const puppeteer = require('puppeteer-core');
const cheerio = require('cheerio');
const fs = require('fs');
const treekill = require('tree-kill');
const dayjs = require('dayjs');
const inquirer = require('./input');

const browserClean = 1;
const browserCleanUnit = 'hour';

const configPath = './config.json';
const userAgent = (process.env.userAgent || 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36');
const proxyAuth = (process.env.proxyAuth || '');
let browserConfig = {
    headless: true,
    args: [
        '--disable-dev-shm-usage',
        '--disable-accelerated-2d-canvas',
        '--no-first-run',
        '--no-zygote',
        '--disable-gpu',
        '--no-sandbox',
        '--disable-setuid-sandbox'
    ]
};

let config = {
    url: null,
    username: null,
    password: null
};
let configured;
let browser;
let page;
let lastBrowserCleanup = dayjs().add(browserClean, browserCleanUnit);

async function initConfig() {
    try {
        console.log('Initializing config...');
        if (fs.existsSync(configPath)) {
            console.log('Json config found!');
            let configFile = JSON.parse(fs.readFileSync(configPath, 'utf8'))
            browserConfig.executablePath = configFile.exec;
            config.url = configFile.url;
            config.username = configFile.username;
            config.password = configFile.password;
            config.gmailUsername = configFile.gmailUsername;
            config.gmailPassword = configFile.gmailPassword;
            config.emailRecipient = configFile.emailRecipient;
        } else if (process.env.url) {
            console.log('Env config found');
            browserConfig.executablePath = '/usr/bin/chromium-browser';
            config.url = process.env.url;
            config.username = process.env.username;
            config.password = process.env.password;
            config.gmailUsername = process.env.gmailUsername;
            config.gmailPassword = process.env.gmailPassword;
            config.emailRecipient = process.env.emailRecipient;
        } else {
            console.log('No config file found!');
            let input = await inquirer.askLogin();
            fs.writeFile(configPath, JSON.stringify(input), function (err) {
                if (err) {
                    console.log(err);
                }
            });
            browserConfig.executablePath = input.exec;
            config.url = input.url;
            config.username = input.username;
            config.password = input.password;
            config.gmailUsername = input.gmailUsername;
            config.gmailPassword = input.gmailPassword;
            config.emailRecipient = input.emailRecipient;
        }
        configured = true;
    } catch (e) {
        console.log('Error: ', e);
    }
}

async function initBrowser(recreate) {
    if (!recreate && browser && page) {
        return;
    }

    browser = await puppeteer.launch(browserConfig);
    page = await browser.newPage();
    await page.setUserAgent(userAgent);

    page.setDefaultNavigationTimeout(process.env.timeout || 0);
    page.setDefaultTimeout(process.env.timeout || 0);

    if (proxyAuth) {
        const credentials = proxyAuth.split(':');
        await page.authenticate({
            'username': credentials[0],
            'password': credentials[1]
        });
    }
}

async function authenticate() {
    if (await isAuthenticated()) {
        return true;
    }

    await page.goto(config.url, {
        waitUntil: 'networkidle0'
    });
    const usernameControl = await page.$('[name="userid"]');
    await usernameControl.type(config.username);
    await page.type('#password', config.password);

    const submitControl = await page.$('[type="submit"]');
    await submitControl.click();
    await page.waitForNavigation();

    if (!await isAuthenticated()) {
        console.log('Authentication failed. Check the credentials.');
        return false;
    }

    return true;
}

async function isAuthenticated() {
    return await page.title() === 'schulNetz';
}

async function loadMarks() {
    await page.reload({
        waitUntil: 'networkidle0'
    });
    const content = await page.content();
    const $ = cheerio.load(content);

    const markTable = $('#content-card > div > div:nth-child(5) > table');
    const marks = $(markTable)
        .find('tbody > tr')
        .toArray()
        .map(x => {
            const rowCellTexts = $(x)
                .find('td')
                .toArray()
                .map(x => cleanText($(x).text()));
            return {
                subject: rowCellTexts[0],
                name: rowCellTexts[1],
                date: rowCellTexts[2],
                value: rowCellTexts[3],
            };
        });

    return marks.some(x => !x.name || !x.date || !x.value) ? [] :
        marks;
}

async function getCurrentMarks() {
    if (!configured) {
        await initConfig();
    }
    await initBrowser();

    if (dayjs(lastBrowserCleanup).isBefore(dayjs())) {
        await cleanup();
        lastBrowserCleanup = dayjs().add(browserClean, browserCleanUnit);
        console.log('Browser cleaned');
    }

    await authenticate(page);
    return await loadMarks(page);
}

async function cleanup() {
    const pages = await browser.pages();
    await pages.map((page) => page.close());
    await treekill(browser.process().pid, 'SIGKILL');
    await initBrowser(true);
}

function cleanText(text) {
    return text.trim();
}

module.exports = {
    getCurrentMarks
};