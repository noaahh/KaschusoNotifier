const nodemailer = require("nodemailer");

let transporter;

function init(config) {
    if (!transporter) {
        transporter = nodemailer.createTransport({
            service: 'gmail',
            host: 'smtp.gmail.com',
            auth: {
                user: config.gmailUsername,
                pass: config.gmailPassword,
            },
        });
    }
}

function createMessageBody(config, marks) {
    let body = '<h2>Your latest marks available for confirmation:</h2><hr>'
    
    marks = marks.sort((a, b) => a.value - b.value);
    marks.forEach(mark => {
        body += `<h3>${mark.name}: ${mark.value}</h3>`;
        body += `<h4>${mark.subject}</h4>`;
        body += '<br>';
    });

    body += `<hr><a href="${config.url}">Confirm marks here 👀</a>`;
    return body;
}

exports.sendMail = async (config, marks) => {
    init(config);
    try {
        await transporter.sendMail({
            from: `"KaschusoNotifier 📢" <${config.gmailUsername}>`,
            to: config.emailRecipient,
            subject: "You have new marks in your Kaschuso ❗",
            html: createMessageBody(config, marks)
        });
        console.log('Mail sent.');
    } catch (e) {
        console.log(e);
        console.log('Mail failed to send. Check your Gmail credentials');
    }
}