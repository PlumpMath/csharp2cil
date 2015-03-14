(function () {
    'use strict';

    var csharpEditor = CodeMirror.fromTextArea(document.getElementById('csharp'), {
        mode: 'text/x-csharp',
        lineNumbers: true
    });

    var cilEditor = CodeMirror.fromTextArea(document.getElementById('cil'), {
        lineNumbers: true,
        readOnly: "nocursor"
    });

    $('button').click(function () {
        var spinner = new Spinner({ length: 20, width: 10, radius: 30 }).spin($('body').get(0));
        $('#fade').show();
        $.post('/home/parse', 'cscode=' + encodeURIComponent(csharpEditor.getValue()), function (types) {
            if (types !== 'error') {
                var cilCode = parseCilCode(types);
                cilEditor.setValue(cilCode.text);
                setHandlers(cilCode.lines);
            } else {
                cilEditor.setValue('syntax error');
            }
        })
        .fail(function () {
            cilEditor.setValue('network problems');
        })
        .always(function () {
            spinner.stop();
            $('#fade').hide();
        });
    });

    function parseCilCode(cilTypes) {
        var lines = [];
        var text = '';
        cilTypes.forEach(function (type) {
            text += '.class ' + type.Name + '\n';
            lines.push(type.Lines);
            text += '{\n';
            lines.push(type.Lines);

            type.Methods.forEach(function (method) {
                text += '    .method ' + method.Name + '\n';
                lines.push(method.Lines);
                text += '    {\n';
                lines.push(method.Lines);

                method.BodyLines.forEach(function (block) {
                    block.Instructions.forEach(function (instruction) {
                        text += '     ' + instruction + '\n';
                        lines.push([block.Line - 1]);
                    });
                });

                text += '    }\n';
                lines.push(method.Lines);
            });

            text += '}\n';
            lines.push(type.Lines);
        });
        return { text: text, lines: lines };
    }

    function setHandlers(lines) {
        var csharpLines = $('.CodeMirror-code').eq(0).children();
        var cilLines = $('.CodeMirror-code').eq(1).children();
        var highlightedLines = [];

        function highlight(line) {
            line.css({ 'background-color': '#17A697' });
            highlightedLines.push(line);
        }

        function removeHighlights() {
            highlightedLines.forEach(function (line) {
                line.css({ 'background-color': 'white' });
            });
            highlightedLines = [];
        }

        csharpLines.hover(function () {
            var currentHover = $(this);
            var lineNumber = currentHover.index();
            highlight(currentHover);

            lines.forEach(function (line, i) {
                if (line.indexOf(lineNumber) !== -1) {
                    highlight(cilLines.eq(i));
                }
            });
        }, removeHighlights);
    }
}());