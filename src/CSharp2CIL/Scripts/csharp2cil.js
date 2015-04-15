(function () {
    'use strict';

    function parseCilCode(cilTypes) {
        var lines = []; //each index represents charp code line number, each value is cil line number
        var text = '';
        var i = 0;

        cilTypes.forEach(function (type) {
            var typeLines = [i++, i++];
            lines[type.Lines[0]] = typeLines;
            lines[type.Lines[1]] = typeLines;
            text += '.class ' + type.Name + '\n';
            text += '{\n';

            type.Methods.forEach(function (method) {
                var methodLines = [i++, i++];
                lines[method.Lines[0]] = methodLines;
                lines[method.Lines[1]] = methodLines;
                text += '    .method ' + method.Name + '\n';
                text += '    {\n';

                method.BodyLines.forEach(function (block) {
                    if (lines[block.Line - 1] === undefined) {
                        lines[block.Line - 1] = [];
                    }
                    block.Instructions.forEach(function (instruction) {
                        lines[block.Line - 1].push(i++);
                        text += '     ' + instruction + '\n';
                    });
                });

                lines[method.Lines[2]] = methodLines;
                methodLines.push(i++);
                text += '    }\n';
            });

            typeLines.push(i++);
            lines[type.Lines[2]] = typeLines;
            text += '}\n';
        });
        return {
            text: text,
            lines: lines
        };
    }

    var App = {
        init: function () {
            this.csharpEditor = CodeMirror.fromTextArea(document.getElementById('csharp'), {
                mode: 'text/x-csharp',
                lineNumbers: true
            });
            this.cilEditor = CodeMirror.fromTextArea(document.getElementById('cil'), {
                lineNumbers: true,
                readOnly: 'nocursor'
            });

            this.lines = [];
            this.highlightedLine = 0;

            this.csharpEditor.on('change', this.removeHighlightHandler.bind(this));
            $('button').on('click', this.parse.bind(this));
        },

        highlight: function (index) {
            this.csharpEditor.addLineClass(index, 'background', 'highlight');

            var cilLinesNumbers = this.lines[index];
            for (var lineNumber in cilLinesNumbers) {
                this.cilEditor.addLineClass(cilLinesNumbers[lineNumber], 'background', 'highlight');
            }
            this.higlighted = index;
        },

        removeHighlight: function () {
            this.csharpEditor.removeLineClass(this.higlighted, 'background', 'highlight');

            var cilLinesNumbers = this.lines[this.higlighted];
            for (var lineNumber in cilLinesNumbers) {
                this.cilEditor.removeLineClass(cilLinesNumbers[lineNumber], 'background', 'highlight');
            }
        },

        removeHighlightHandler: function () {
            this.removeHighlight();
            $('.CodeMirror-code >', this.csharpEditor.getWrapperElement()).unbind('mouseenter mouseleave');
        },

        parse: function () {
            var spinner = new Spinner({
                length: 20,
                width: 10,
                radius: 30
            }).spin($('body').get(0));
            var self = this;
            var csCode = encodeURIComponent(this.csharpEditor.getValue());

            $('#fade').show();
            $.post('/home/parse', 'cscode=' + csCode, function (types) {
                if (types !== 'error') {
                    var cilCode = parseCilCode(types);

                    this.cilEditor.setValue(cilCode.text);
                    this.lines = cilCode.lines;

                    var csharpLines = $('.CodeMirror-code >', this.csharpEditor.getWrapperElement());
                    csharpLines.on('mouseenter', function () {
                        var i = $(this).index();
                        self.highlight(i);
                    });

                    csharpLines.on('mouseleave', this.removeHighlight.bind(this));

                } else {
                    this.cilEditor.setValue('syntax error');
                }

                spinner.stop();
                $('#fade').hide();
            }.bind(this));
        }
    };

    App.init();
}());