const fs = require('node:fs');
const path = require('node:path');
const ts = require('typescript');
const root = path.resolve(__dirname, '../src');
const nativeNames = new Set(['alert', 'confirm', 'prompt']);
const globals = new Set(['window', 'globalThis', 'self']);
const violations = [];
let files = 0;
function walk(directory) {
  for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
    const file = path.join(directory, entry.name);
    if (entry.isDirectory()) { walk(file); continue; }
    if (!file.endsWith('.ts') || file.endsWith('.spec.ts')) continue;
    files++;
    const source = ts.createSourceFile(file, fs.readFileSync(file, 'utf8'), ts.ScriptTarget.Latest, true);
    function visit(node) {
      if (ts.isCallExpression(node)) {
        const expression = node.expression;
        const bare = ts.isIdentifier(expression) && nativeNames.has(expression.text);
        const globalCall = (ts.isPropertyAccessExpression(expression) || ts.isElementAccessExpression(expression)) &&
          ts.isIdentifier(expression.expression) && globals.has(expression.expression.text) &&
          nativeNames.has(ts.isPropertyAccessExpression(expression) ? expression.name.text :
            ts.isStringLiteral(expression.argumentExpression) ? expression.argumentExpression.text : '');
        if (bare || globalCall) {
          const position = source.getLineAndCharacterOfPosition(node.getStart(source));
          violations.push(`${path.relative(root, file)}:${position.line + 1}`);
        }
      }
      ts.forEachChild(node, visit);
    }
    visit(source);
  }
}
walk(root);
if (violations.length) {
  console.error('Use ActionDialogService instead of browser dialogs:\n' + violations.join('\n'));
  process.exitCode = 1;
} else console.log(`Checked ${files} TypeScript files: no native alert, confirm or prompt calls.`);
