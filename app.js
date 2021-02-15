require('dotenv').config();
require('log-timestamp');
const puppeteer = require('puppeteer-core');
const cheerio = require('cheerio');
const fs = require('fs');
const treekill = require('tree-kill');
const dayjs = require('dayjs');

const inquirer = require('./input');
const mail = require('./mail');

let run = true;

// Config
const configPath = './config.json'
const userAgent = (process.env.userAgent || 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36');

const proxy = (process.env.proxy || ""); // ip:port
const proxyAuth = (process.env.proxyAuth || "");

const browserClean = 1;
const browserCleanUnit = 'hour';

const marksFilePath = './marks.json';
let discoveredMarks = [];

let config = {
    url: null,
    username: null,
    password: null
};

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

async function initConfig() {
    try {
        if (proxy) browserConfig.args.push('--proxy-server=' + proxy);

        console.log('🔎 Checking config file...');
        if (fs.existsSync(configPath)) {
            console.log('✅ Json config found!');
            let configFile = JSON.parse(fs.readFileSync(configPath, 'utf8'))
            browserConfig.executablePath = configFile.exec;
            config.url = configFile.url;
            config.username = configFile.username;
            config.password = configFile.password;
            config.gmailUsername = configFile.gmailUsername;
            config.gmailPassword = configFile.gmailPassword;
            config.emailRecipient = configFile.emailRecipient;
        } else if (process.env.url) {
            console.log('✅ Env config found');
            browserConfig.executablePath = '/usr/bin/chromium-browser';
            config.url = process.env.url;
            config.username = process.env.username;
            config.password = process.env.password;
            config.gmailUsername = process.env.gmailUsername;
            config.gmailPassword = process.env.gmailPassword;
            config.emailRecipient = process.env.emailRecipient;
        } else {
            console.log('❌ No config file found!');
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
    } catch (e) {
        console.log('Error: ', e);
    }
}

async function spawnBrowser() {
    console.log("=========================");
    console.log('📱 Launching browser...');
    var browser = await puppeteer.launch(browserConfig);
    var page = await browser.newPage();

    console.log('🔧 Setting User-Agent...');
    await page.setUserAgent(userAgent); //Set userAgent

    console.log('⏰ Setting timeouts...');
    page.setDefaultNavigationTimeout(process.env.timeout || 0);
    page.setDefaultTimeout(process.env.timeout || 0);

    if (proxyAuth) {
        await page.setExtraHTTPHeaders({
            'Proxy-Authorization': 'Basic ' + Buffer.from(proxyAuth).toString('base64')
        })
    }

    return {
        browser,
        page
    };
}

async function login(page) {
    await page.goto(config.url, {
        waitUntil: 'networkidle0'
    });
    const usernameControl = await page.$('[name="userid"]');
    await usernameControl.type(config.username);
    await page.type('#password', config.password);

    const submitControl = await page.$('[type="submit"]');
    await submitControl.click();
    await page.waitForNavigation();

    const pageTitle = await page.title();
    if (pageTitle !== 'schulNetz') {
        console.log('🛑 Login failed!');
        console.log('🔑 Invalid credentials!');
        process.exit();
    }

    console.log('✅ Login successful!');
    return true;
}

async function getCurrentMarks(page) {
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

async function checkNewMarks(browser, page) {
    let lastBrowserCleanup = dayjs().add(browserClean, browserCleanUnit);
    let firstRun = true;
    while (run) {
        if (dayjs(lastBrowserCleanup).isBefore(dayjs())) {
            console.log('Cleaning browser...');
            const newSpawn = await cleanup(browser);
            browser = newSpawn.browser;
            page = newSpawn.page;
            firstRun = true;
            lastBrowserCleanup = dayjs().add(browserClean, browserCleanUnit);
            console.log('Browser cleaned.');
        }

        if (firstRun) {
            console.log('🔐 Checking login...');
            await login(page);
            firstRun = false;
        }

        console.log('Checking for new marks...');
        const newMarks = [];
        const currentMarks = await getCurrentMarks(page);
        currentMarks.forEach(mark => {
            if (!discoveredMarks.some(x => x.subject === mark.subject && x.name === mark.name && x.value === mark.value)) {
                newMarks.push(mark);
            }
        });

        if (newMarks && newMarks.length > 0) {
            console.log(`${newMarks.length} marks available.`);
            await onNewMarks(newMarks);
        } else {
            console.log(`Couldn't find any new marks.`);
        }

        await page.waitFor(60000);
    }
}

async function onNewMarks(newMarks) {
    await mail.sendMail(config, newMarks);

    discoveredMarks.push(...newMarks);
    fs.writeFileSync(marksFilePath, JSON.stringify(discoveredMarks));
}

async function shutDown() {
    run = false;
    process.exit();
}

async function cleanup(browser) {
    const pages = await browser.pages();
    await pages.map((page) => page.close());
    await treekill(browser.process().pid, 'SIGKILL');
    return await spawnBrowser();
}

async function init() {
    if (fs.existsSync(marksFilePath)) {
        discoveredMarks = JSON.parse(fs.readFileSync(marksFilePath));
    }
    cookie = await initConfig();
}

async function main() {
    await init();

    var {
        browser,
        page
    } = await spawnBrowser();

    console.log("=========================");
    console.log('🔭 Running notifier...');

    await checkNewMarks(browser, page);
    await browser.close();
};

function cleanText(text) {
    return text.trim();
}

main();

process.on("SIGINT", shutDown);
process.on("SIGTERM", shutDown);
