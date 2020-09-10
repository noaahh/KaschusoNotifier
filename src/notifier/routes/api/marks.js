var router = require('express').Router();
var auth = require('../auth');
var notifier = require('../../services/notifier');
var kaschuso = require('../../services/kaschuso');
var mongoose = require('mongoose');
var Mark = mongoose.model('Mark');

router.get('/new', auth.required, async function (req, res, next) {
    return res.json(await notifier.getNewMarks());
});

router.get('/current', auth.required, async function (req, res, next) {
    return res.json(await kaschuso.getCurrentMarks());
});

router.get('/', auth.required, async function (req, res, next) {
    return res.json(await Mark.find());
});

module.exports = router;