var gulp = require("gulp"),
    bundleTask = require('./node_modules/geonorge-shared-partials/gulp-tasks/bundle')(gulp),
    config = {
        url: './node_modules/geonorge-shared-partials/bundle.config.js',
        dist: 'dist'
    };

gulp.task("default", function () { return bundleTask(config) });
gulp.task("test", function () { bundleTask(config, "test") });
gulp.task("prod", function () { bundleTask(config, "prod") });