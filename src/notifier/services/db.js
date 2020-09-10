var mongoose = require('mongoose');
var Mark = mongoose.model('Mark');

async function saveMark(mark) {
    mark.discovered = new Date();
    await mark.save();
}

async function markExists({
    subject,
    name,
    value
}) {
    const result = await Mark.find({
        'subject': subject,
        'name': name,
        'value': value
    }).exec();
    return result && result.length > 0;
}

module.exports = {
    saveMark,
    markExists
}