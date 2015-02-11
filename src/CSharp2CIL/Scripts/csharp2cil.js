var csharpEditor = CodeMirror.fromTextArea(document.getElementById('csharp'), {
    mode: 'text/x-csharp',
    lineNumbers: true
});

var cilEditor = CodeMirror.fromTextArea(document.getElementById('cil'), {
    lineNumbers: true
});

var editorLines = [];

function setEditorText(cilTypes) {
    var text = '';
    cilTypes.forEach(function (type) {
        text += '.class ' + type.Name + '\n';
        text += '{\n';
        editorLines.push([type.StartLine, type.EndLine]);
        editorLines.push([type.StartLine, type.EndLine]);

        type.CilMethods.forEach(function (method) {
            text += '    .method ' + method.Name + '\n';
            text += '    {\n';
            editorLines.push([method.StartLine, method.EndLine]);
            editorLines.push([method.StartLine, method.EndLine]);

            method.CilLineInsturctions.forEach(function (block) {
                block.Instructions.forEach(function (instruction) {
                    text += '     ' + instruction + '\n';
                    editorLines.push([block.Line - 1]);
                });
            });

            text += '    }\n';
            editorLines.push([method.StartLine, method.EndLine]);
        });

        text += '}\n';
        editorLines.push([type.StartLine, type.EndLine]);
    });

    cilEditor.setValue(text);
}

function setHandlers() {
    var highlightedLines = [];
    var csharpLines = $('.CodeMirror-code').eq(0).children();
    var cilLines = $('.CodeMirror-code').eq(1).children();

    function highlight(line) {
        line.css({ 'background-color': '#00a855' });
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

        editorLines.forEach(function (editorLine, i) {
            if (editorLine.indexOf(lineNumber) !== -1) {
                highlight(cilLines.eq(i));
            }
        });
    }, removeHighlights);

    cilLines.hover(function () {
        var currentHover = $(this);
        var lineNumber = currentHover.index();
        highlight(currentHover);

        if (editorLines[lineNumber] !== undefined) {
            editorLines[lineNumber].forEach(function (editorLine) {
                highlight(csharpLines.eq(editorLine));
            });
        };
    }, removeHighlights);
}

function parse() {
    $.post('/Home/Parse', 'csCode=' + encodeURIComponent(csharpEditor.getValue()), function (types) {
        if (types !== 'error') {
            editorLines = [];
            setEditorText(types);
            setHandlers();
        } else {
            cilEditor.setValue('error');
        }
    });
}