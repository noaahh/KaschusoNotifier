const kaschuso = require('./kaschuso');
const {
    markExists,
    saveMark,
    Mark
} = require('./db');

async function startFrequentCheck() {
    checkNewMarks();
    setInterval(() => {
        checkNewMarks();
    }, 60000);
};

async function checkNewMarks() {
    const newMarks = await getNewMarks();
    await Promise.all(newMarks.map(x => saveMark(new Mark(x))));
}

async function getNewMarks() {
    const currentMarks = await kaschuso.getCurrentMarks();
    const newMarks = [];
    for (let i = 0; i < currentMarks.length; i++) {
        const mark = currentMarks[i];
        if (!await markExists(mark)) {
            newMarks.push(mark);
        }
    }
    return newMarks;
}

module.exports = {
    startFrequentCheck,
    getNewMarks
}