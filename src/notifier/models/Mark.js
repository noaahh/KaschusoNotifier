var mongoose = require('mongoose');

const MarkSchema = new mongoose.Schema({
    discovered: Date,
    subject: String,
    name: String,
    value: Number
}, {
    timestamps: true
});

MarkSchema.methods.toJSONFor = function () {
    return {
        discovered: this.discovered,
        subject: this.subject,
        name: this.name,
        value: this.value
    };
};

mongoose.model('Mark', MarkSchema);