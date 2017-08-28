var gulp = require('gulp');

// all gulp plugins
var gulpExec = require('gulp-exec');
var gulpClean = require('gulp-clean');

gulp.task('clean:dist', function () {
    return gulp.src('./dist/**/*')
        .pipe(gulpClean());
});

gulp.task('build:server', function () {
    return gulp.src('./main.go')
        .pipe(gulpExec('go build -o ./dist/bin/fileharbor <%= file.path %>'))
        .pipe(gulpExec.reporter());
});

gulp.task('release', ['clean', 'build'], function () {
    return gulp.src('./config/templates/**/*')
        .pipe(gulp.dest('./dist/templates'));
});

gulp.task('build', ['build:server']);
gulp.task('clean', ['clean:dist']);