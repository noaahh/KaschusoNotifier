const inquirer = require('inquirer');
exports.askLogin = () => {
    const questions = [{
            name: 'url',
            type: 'input',
            message: 'Enter your Kaschuso url (e.g. https://kaschuso.so.ch/gibsso):',
            validate: function (value) {
                if (value.length) {
                    return true;
                } else {
                    return 'Please enter a valid url!';
                }
            }
        },
        {
            name: 'username',
            type: 'input',
            message: 'Enter your Kaschuso username:',
            validate: function (value) {
                if (value.length) {
                    return true;
                } else {
                    return 'Please enter a valid username!';
                }
            }
        },
        {
            name: 'password',
            type: 'password',
            message: 'Enter your Kaschuso password 🔑:',
            validate: function (value) {
                if (value.length) {
                    return true;
                } else {
                    return 'Please enter a valid password!';
                }
            }
        },
        {
            name: 'gmailUsername',
            type: 'input',
            message: 'Enter your Gmail username:',
            validate: function (value) {
                if (value.length) {
                    return true;
                } else {
                    return 'Please enter a valid username!';
                }
            }
        },
        {
            name: 'gmailPassword',
            type: 'password',
            message: 'Enter your Gmail password 🔑:',
            validate: function (value) {
                if (value.length) {
                    return true;
                } else {
                    return 'Please enter a valid password!';
                }
            }
        },
        {
            name: 'emailRecipient',
            type: 'input',
            message: 'Enter the recipient email:',
            validate: function (value) {
                if (value.length) {
                    return true;
                } else {
                    return 'Please enter a valid email!';
                }
            }
        },
        {
            name: 'exec',
            type: 'input',
            message: 'Enter the chromium executable path (usually /usr/bin/chromium-browser or /usr/bin/chromium or C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe):',
            validate: function (value) {
                if (value.length) {
                    return true;
                } else {
                    return 'Please enter your valid path!';
                }
            }
        }
    ];
    return inquirer.prompt(questions);
};