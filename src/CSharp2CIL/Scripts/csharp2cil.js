var editor = CodeMirror.fromTextArea(document.getElementById('csharp'), {
    mode: "text/x-csharp",
    lineNumbers: true
});
editor.setSize(null, '97%');

var editor2 = CodeMirror.fromTextArea(document.getElementById('cil'), {
    lineNumbers: true
});
editor2.setSize(null, '97%');

var cilLines = [];
function parse() {
    $.post('/Home/Parse', 'csCode=' + encodeURIComponent(editor.getValue()), function (types) {
        if (types !== "error") {
            var text = "";
            var lineNumber = 0;
            for (var i = 0; i < types.length; i++) {
                var type = types[i];
                csharpClassDeclarationLines = [type.StartLine + 1, type.EndLine + 1];
                cilLines[lineNumber++] = csharpClassDeclarationLines; //class name
                cilLines[lineNumber++] = csharpClassDeclarationLines;
                text += '.class ' + type.Name + '\n';
                text += '{\n';

                //methods
                type.CilMethods.forEach(function (method) {
                    var methodDeclarationLines = [method.StartLine + 1, method.EndLine + 1];
                    cilLines[lineNumber++] = methodDeclarationLines;
                    cilLines[lineNumber++] = methodDeclarationLines;
                    text += '    .method ' + method.Name + '\n';
                    text += '    {\n';
                    //body
                    method.CilLineInsturctions.forEach(function (block) {
                        block.Instructions.forEach(function (instruction) {
                            text += '     ' + instruction + '\n';
                            cilLines[lineNumber++] = [block.Line];
                        });
                    });

                    cilLines[lineNumber++] = methodDeclarationLines;
                    text += '    }\n';
                });

                cilLines[lineNumber++] = csharpClassDeclarationLines;
                text += '}\n';
            }

            editor2.setValue(text);


            //-------------------- handler to csharp code
            var hovered = [];
            var currentHover;
            $('>', $('.CodeMirror-code').eq(0)).hover(function () {

                var lineNumber = parseInt(this.innerText);
                for (var i = 0; i < cilLines.length; i++) {
                    if (cilLines[i].indexOf(parseInt(lineNumber)) != -1) {
                        var line = $('> div:nth-child(' + (i + 1) + ')', $('.CodeMirror-code').eq(1));
                        line.css({ 'background-color': '#00a855' });
                        hovered.push(line);
                    }

                }
                currentHover = $('> div:nth-child(' + lineNumber + ')', $('.CodeMirror-code').eq(0));
                currentHover.css({ 'background-color': '#00a855' });


            }, function () {
                hovered.forEach(function(line) {
                    line.css({ 'background-color': 'white' });
                });
                hovered = [];
                currentHover.css({ 'background-color': 'white' });
            });


            //----------handler to cil code
            //var hovered = [];
            $('>', $('.CodeMirror-code').eq(1)).hover(function () {
                var lineNumber = parseInt(this.innerText) - 1;
                if (cilLines[lineNumber] !== undefined) {
                    cilLines[lineNumber].forEach(function (csharpLine) {
                        var line = $('> div:nth-child(' + csharpLine + ')', $('.CodeMirror-code').eq(0));
                        line.css({ 'background-color': '#00a855' });
                        hovered.push(line);
                    })
                    currentHover = $('> div:nth-child(' + (lineNumber + 1) + ')', $('.CodeMirror-code').eq(1));
                    currentHover.css({ 'background-color': '#00a855' });

                }

            }, function () {
                hovered.forEach(function (line) {
                    line.css({ 'background-color': 'white' });
                });
                hovered = [];
                currentHover.css({ 'background-color': 'white' });
            });
        } else {
            editor2.setValue('error');
        }

    });

}