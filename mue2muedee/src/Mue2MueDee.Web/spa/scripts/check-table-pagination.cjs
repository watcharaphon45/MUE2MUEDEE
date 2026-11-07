const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const root = path.resolve(__dirname, '../src/app/features');
let tables = 0;
let printed = 0;
function check(directory) {
  for (const file of fs.readdirSync(directory, { withFileTypes: true })) {
    const fullPath = path.join(directory, file.name);
    if (file.isDirectory()) check(fullPath);
    else if (file.name.endsWith('.html')) {
      const source = fs.readFileSync(fullPath, 'utf8');
      let paginated = 0;
      for (const match of source.matchAll(/<table\b[\s\S]*?<\/table>/g)) {
        const table = match[0];
        if (/^<table[^>]*class="print-/.test(table)) {
          printed++;
          assert(!table.includes('.pagedItems()'), `Printed document must contain all rows: ${fullPath}`);
          continue;
        }
        tables++;
        paginated++;
        assert(/^<table[^>]*\bapp-table\b/.test(table), `Missing shared column sizing: ${fullPath}`);
        assert(table.includes('.pagedItems()') || table.includes('.pagedResult().items'), `Rows are not paginated: ${fullPath}`);
        assert(!/<th(?:\s[^>]*)?>/.test(table.replace(/<th[^>]*class="[^"]*table-(?:text|code|money|date|number|compact|actions|check)[^"]*"[^>]*>/g, '')), `Missing column sizing: ${fullPath}`);
      }
      const controls = [...source.matchAll(/<app-table-pagination\b/g)].length;
      assert(controls >= paginated, `Missing pagination controls: ${fullPath}`);
    }
  }
}
check(root);
assert.equal(tables, 20, 'Review the coverage list when adding a new table');
assert.equal(printed, 2, 'Review printed document coverage');
console.log(`Checked ${tables} interactive tables and ${printed} complete printed tables.`);
