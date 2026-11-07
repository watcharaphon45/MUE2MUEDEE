// Real-browser layout checks against a local mock API. No request reaches a shop database.
const fs = require('node:fs');
const path = require('node:path');
const os = require('node:os');
const http = require('node:http');
const { spawn } = require('node:child_process');
const assert = require('node:assert/strict');
const dist = path.resolve(__dirname, '../dist/frontend/browser');
const output = path.resolve(__dirname, '../.angular/table-ui-check');
const stamp = '2026-11-07T02:00:00Z';
const id = index => `00000000-0000-4000-8000-${String(index + 1).padStart(12, '0')}`;
const rows = Array.from({ length: 61 }, (_, index) => ({
  id: id(index), userId: id(index), inventoryItemId: id(index), orderId: id(index), purchaseOrderId: id(index),
  name: `ตัวอย่าง ${index + 1} ร้านกล้องและอุปกรณ์ถ่ายภาพมือสอง`, fullName: `ผู้ใช้งานตัวอย่าง ${index + 1}`, username: `user${index + 1}`,
  code: `SHOP-${index + 1}`, sku: `CAM-2611-${String(index + 1).padStart(4, '0')}`, serialNumber: `SN-ALPHA-7-IV-${index + 1}`,
  productId: id(0), brandId: id(0), categoryId: id(0), conditionGradeId: id(0),
  modelName: `Alpha 7 IV รุ่นทดสอบ ${index + 1}`, productModelName: `Alpha 7 IV รุ่นทดสอบ ${index + 1}`, productCode: `SONY-A7IV-${index + 1}`,
  brandName: 'Sony', categoryName: 'กล้องถ่ายภาพ', categoryType: 1, type: 1, typeName: 'กล้อง',
  gradeCode: 'A', gradeName: 'สภาพดีมาก', conditionGradeCode: 'A', conditionGradeName: 'สภาพดีมาก', conditionGrade: 'A',
  customerType: 3, customerTypeName: 'ซื้อและขาย', customerName: `ลูกค้าตัวอย่าง ${index + 1} ชื่อยาวสำหรับทดสอบการแสดงผล`, customerPhone: '0812345678',
  phone: '0812345678', phoneNumber: '0812345678', email: `long.customer.name.${index + 1}@example.test`, address: 'ที่อยู่ตัวอย่าง กรุงเทพฯ',
  ownerFullName: `เจ้าของร้านตัวอย่าง ${index + 1}`, ownerUsername: `owner${index + 1}`, ownerEmail: `owner${index + 1}@example.test`, ownerPhoneNumber: '0812345678',
  description: index % 5 === 0 ? 'รายละเอียดสินค้าตัวอย่างที่มีข้อความยาว เพื่อทดสอบการตัดบรรทัดให้พอดีกับคอลัมน์โดยไม่ซ่อนข้อมูลที่จำเป็น' : 'ข้อมูลตัวอย่าง',
  notes: 'หมายเหตุทดสอบ', accessories: 'กล่อง แบตเตอรี่ และสายคล้อง', defects: 'ไม่มีตำหนิสำคัญ',
  status: 3, statusName: 'พร้อมขาย', isActive: true, role: 'Owner', userCount: 12, productCount: 25, sourcedItemCount: 4, inventoryCount: 5, sortOrder: index + 1,
  purchaseCost: 50000, actualCost: 51500, sellingPrice: index % 7 === 0 ? 1234567.89 : 64900,
  defaultSellingPrice: 64900, expectedProfit: 13400, expectedMarginPercent: 20.65,
  expectedSalePrice: 64900, estimatedRepairCost: 1000, estimatedCleaningCost: 500, maxBuyPrice: 54000, offeredPrice: 50000, agreedPrice: 50000,
  unitPrice: 64900, discount: 100, subTotal: 64800, grossProfit: 13300, marginPercentage: 20.52, warrantyDays: 30,
  amount: 50000, paidAt: stamp, paymentMethod: 1, paymentMethodName: 'เงินสด', referenceNumber: `REF-${index + 1}`,
  costType: 2, costTypeName: 'ค่าซ่อม', incurredAt: stamp, createdBy: 'ผู้บันทึกตัวอย่าง', changedBy: 'ผู้จัดการร้าน', changedAt: stamp,
  oldPrice: 64000, newPrice: 64900, priceDifference: 900, reason: 'ปรับตามสภาพสินค้าและราคาตลาด',
  createdAt: stamp, purchaseDate: stamp, receivedAt: stamp, saleDate: stamp, joinedAt: stamp,
  poNumber: `PO-2611-${String(index + 1).padStart(4, '0')}`, saleNumber: `SO-2611-${String(index + 1).padStart(4, '0')}`,
  needsApproval: false, totalEstimatedValue: 64900, totalMaxBuyPrice: 54000, totalOfferedPrice: 50000, totalAgreedPrice: 50000,
  totalAmount: 64900, discountAmount: 100, netAmount: 64800, totalCost: 51500, totalGrossProfit: 13300, totalMarginPercentage: 20.52,
  shutterCount: 5000, daysInStock: 80 + index, currentSellingPrice: 64900, recommendedClearancePrice: 60000, agingBucket: '91–120 วัน',
  agingDiscountPercentage: 10, orderNumber: `SO-2611-${String(index + 1).padStart(4, '0')}`, orderDate: stamp, itemCount: 2, paymentMethods: 'เงินสด, โอนผ่านธนาคาร',
  inspectionResults: [], imageUrls: [], images: [], costs: [], movements: [], primaryColor: '#0284c7'
}));
const store = { ...rows[0], id: id(100), name: 'ร้านตัวอย่างสำหรับตรวจตาราง', code: 'AUDIT', ownerName: 'เจ้าของร้าน', ownerPhone: '0812345678' };
const purchases = rows.map(row => ({ ...row, items: rows.slice(0, 2), payments: [], status: 3 }));
const sales = rows.map(row => ({ ...row, items: rows.slice(0, 2), payments: [], status: 2, statusName: 'เสร็จสมบูรณ์' }));
const paged = (items, url) => {
  const page = Number(url.searchParams.get('page') || 1), pageSize = Number(url.searchParams.get('pageSize') || 15);
  return { items: items.slice((page - 1) * pageSize, page * pageSize), totalCount: items.length, page, pageSize, totalPages: Math.ceil(items.length / pageSize) };
};
function api(url) {
  const p = url.pathname;
  if (p === '/api/stores/current') return store;
  if (p === '/api/stores/users') return rows;
  if (p.startsWith('/api/master-data/') || p === '/api/customers') return rows;
  if (p === '/api/super-admin/stores') return rows;
  if (p === '/api/super-admin/stats') return { totalStores: 61, activeStores: 61, totalOwners: 61, totalUsers: 61 };
  if (p === '/api/inventory') return paged(rows, url);
  if (/\/inventory\/[^/]+\/costs$/.test(p)) return { ...rows[0], modelName: 'Alpha 7 IV', repairCost: 1000, cleaningCost: 500, shippingCost: 0, otherCost: 0, totalActualCost: 51500, costItems: rows };
  if (p.endsWith('/price-history')) return rows;
  if (p.endsWith('/movements')) return [];
  if (p.endsWith('/pricing-recommendation')) return { ...rows[0], recommendedPrice: 64900, minimumPrice: 55000, maximumPrice: 70000, breakEvenPrice: 51500, agingDays: 90, targetMarginPercentage: 15, estimatedGrossProfit: 13400, estimatedMarginPercentage: 20.65, agingColorCode: 'amber', pricingRationale: 'ข้อมูลตัวอย่าง' };
  if (p.startsWith('/api/inventory/')) return rows[0];
  if (p === '/api/purchases') return paged(purchases, url);
  if (p.startsWith('/api/purchases/')) return { ...purchases[0], items: rows, payments: rows };
  if (p === '/api/sales') return paged(sales, url);
  if (p.startsWith('/api/sales/')) return { ...sales[0], items: rows, payments: rows };
  if (p === '/api/sticker-templates') return [];
  if (p.endsWith('/sales-report')) return { summary: { orderCount: 61, itemsSoldCount: 122, totalGrossSales: 3958900, totalDiscount: 6100, totalNetSales: 3952800, totalCost: 3141500, totalGrossProfit: 811300, averageMarginPercentage: 20.52, isFinancialVisible: true }, brandBreakdown: [], categoryBreakdown: [], paymentBreakdown: [], dailyTrend: [], orders: rows };
  if (p.endsWith('/stock-aging')) return { totalItemsInStock: 61, isFinancialVisible: true, totalCostValue: 3141500, totalSalesValue: 3958900, deadCapitalCostValue: 515000, buckets: [], agingItems: rows };
  if (p.startsWith('/api/catalog/')) {
    const result = paged(rows, url);
    return { store, ...result, currentPage: result.page, filterMeta: { brands: [], categories: [], conditionGrades: [], totalAvailableItems: 61 } };
  }
  throw new Error(`Mock API route missing: ${p}`);
}

(async () => {
  fs.mkdirSync(output, { recursive: true });
  const missing = [];
  const server = http.createServer((req, res) => {
    const url = new URL(req.url, 'http://localhost');
    if (url.pathname.startsWith('/api/')) {
      // All mutations are blocked by this mock server.
      if (req.method !== 'GET') { res.writeHead(405).end(); return; }
      try { res.setHeader('Content-Type', 'application/json'); res.end(JSON.stringify({ success: true, statusCode: 200, data: api(url), message: 'Mock data', errors: null, timestamp: stamp })); }
      catch (error) { missing.push(error.message); res.writeHead(404).end('{}'); }
      return;
    }
    if (url.pathname === '/__fixtures') { res.setHeader('Content-Type', 'application/json'); res.end(JSON.stringify(rows)); return; }
    let file = path.resolve(dist, '.' + decodeURIComponent(url.pathname));
    if (!file.startsWith(dist + path.sep)) { res.writeHead(403).end(); return; }
    if (!fs.existsSync(file) || fs.statSync(file).isDirectory()) file = path.join(dist, 'index.html');
    res.setHeader('Content-Type', ({ '.js': 'text/javascript', '.css': 'text/css', '.html': 'text/html', '.svg': 'image/svg+xml', '.png': 'image/png' })[path.extname(file)] || 'application/octet-stream');
    fs.createReadStream(file).pipe(res);
  });
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  const origin = `http://127.0.0.1:${server.address().port}`;
  const profile = fs.mkdtempSync(path.join(os.tmpdir(), 'mue2-table-browser-'));
  const browserPath = ['C:/Program Files/Google/Chrome/Application/chrome.exe', 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe'].find(file => fs.existsSync(file));
  assert(browserPath, 'Chrome or Edge is required for browser checks');
  const browser = spawn(browserPath, ['--headless=new', '--disable-gpu', '--no-first-run', '--no-default-browser-check', '--remote-debugging-port=0', `--user-data-dir=${profile}`, 'about:blank'], { windowsHide: true, stdio: 'ignore' });
  const sleep = ms => new Promise(resolve => setTimeout(resolve, ms));
  async function until(check, label) {
    for (let i = 0; i < 100; i++) { if (await check()) return; await sleep(150); }
    throw new Error(`Timed out: ${label}`);
  }
  let ws;
  try {
    await until(() => fs.existsSync(path.join(profile, 'DevToolsActivePort')), 'browser startup');
    const port = fs.readFileSync(path.join(profile, 'DevToolsActivePort'), 'utf8').split('\n')[0];
    const targets = await (await fetch(`http://127.0.0.1:${port}/json/list`)).json();
    ws = new WebSocket(targets.find(target => target.type === 'page').webSocketDebuggerUrl);
    await new Promise(resolve => ws.addEventListener('open', resolve, { once: true }));
    let sequence = 0;
    let completedLoads = 0;
    const pending = new Map();
    const errors = [];
    ws.addEventListener('message', event => {
      const message = JSON.parse(event.data);
      if (message.id) {
        const task = pending.get(message.id); pending.delete(message.id);
        if (task) message.error ? task.reject(new Error(message.error.message)) : task.resolve(message.result);
      } else if (message.method === 'Runtime.exceptionThrown') errors.push(message.params.exceptionDetails.exception?.description || message.params.exceptionDetails.text);
      else if (message.method === 'Page.loadEventFired') completedLoads++;
    });
    const send = (method, params = {}) => new Promise((resolve, reject) => { const requestId = ++sequence; pending.set(requestId, { resolve, reject }); ws.send(JSON.stringify({ id: requestId, method, params })); });
    const evaluate = async expression => {
      const result = await send('Runtime.evaluate', { expression, awaitPromise: true, returnByValue: true });
      if (result.exceptionDetails) throw new Error(result.exceptionDetails.exception?.description || expression);
      return result.result.value;
    };
    await send('Page.enable'); await send('Runtime.enable');
    const storeInfo = { storeId: store.id, storeName: store.name, storeCode: store.code, role: 'Owner', primaryColor: '#0284c7' };
    const storage = { mue2_access_token: 'local-fixture-token', mue2_refresh_token: 'local-fixture-refresh', mue2_user: JSON.stringify({ id: id(101), username: 'fixture', fullName: 'ผู้ตรวจตัวอย่าง', isSuperAdmin: true }), mue2_current_store: JSON.stringify(storeInfo), mue2_stores: JSON.stringify([storeInfo]) };
    await send('Page.addScriptToEvaluateOnNewDocument', { source: `Object.entries(${JSON.stringify(storage)}).forEach(([key,value])=>localStorage.setItem(key,value));` });
    const views = [
      ['inventory', '/inventory'], ['purchase', '/purchase'], ['sales', '/sales'],
      ...['products', 'brands', 'categories', 'grades', 'customers'].map(tab => [`master-${tab}`, `/master-data?tab=${tab}`]),
      ['customers', '/customers'], ['users', '/settings', 'app-store-settings', "c.activeTab.set('users')"], ['stores', '/super-admin/stores'],
      ['report-sales', '/reports'], ['report-aging', '/reports', 'app-reports-dashboard', "c.setTab('aging')"],
      ['inventory-costs', `/inventory/${id(0)}`, 'app-inventory-detail', "c.activeTab.set('costs')"],
      ['inventory-prices', `/inventory/${id(0)}`, 'app-inventory-detail', "c.activeTab.set('pricing')"],
      ['purchase-detail', `/purchase/${id(0)}`], ['sale-detail', `/sales/${id(0)}`],
      ['pos', '/sales/pos', 'app-pos', "c.cartItems.set((await (await fetch('/__fixtures')).json()).slice(0,32).map(item=>({inventoryItem:item,unitPrice:1000,discount:0,subTotal:1000,grossProfit:250,marginPercentage:25,warrantyDays:30})))"],
      ['catalog', '/shop/AUDIT']
    ];
    const results = [];
    for (const width of [1440, 390]) {
      await send('Emulation.setDeviceMetricsOverride', { width, height: 960, deviceScaleFactor: 1, mobile: width < 600 });
      for (const [name, route, selector, action] of views) {
        const previousLoad = completedLoads;
        await send('Page.navigate', { url: origin + route });
        await until(() => completedLoads > previousLoad, name + ' document load');
        await until(() => evaluate("!!document.querySelector('app-table-pagination') || !!window.ng && !!document.querySelector('" + (selector || 'app-root') + "')"), name + ' load');
        if (selector) {
          await until(() => evaluate(`(()=>{const e=document.querySelector('${selector}');return !!e && e.children.length>0 && !!window.ng?.getComponent(e)})()`), name + ' component');
          await evaluate(`(async()=>{ const c=window.ng.getComponent(document.querySelector('${selector}')); ${action}; window.ng.applyChanges(c); })()`);
        }
        try {
          await until(() => evaluate("Array.from(document.querySelectorAll('app-table-pagination .page-range')).some(e=> /จาก (61|32) รายการ/.test(e.textContent))"), name + ' data');
        } catch (error) {
          console.error(await evaluate("({url:location.href,text:document.body.innerText.slice(-5000)})"), errors, missing);
          throw error;
        }
        await sleep(100);
        const state = await evaluate(`(()=>({ width:innerWidth, documentWidth:document.documentElement.scrollWidth, tables:[...document.querySelectorAll('table.app-table')].filter(t=>t.getClientRects().length).map(t=>({rows:t.querySelectorAll('tbody tr').length,scroll:t.parentElement.scrollWidth>t.parentElement.clientWidth})),ranges:[...document.querySelectorAll('.page-range')].map(e=>e.textContent.trim()) }))()`);
        assert(state.documentWidth <= width + 2, `${name} overflows viewport at ${width}: ${state.documentWidth}`);
        for (const table of state.tables) assert(table.rows <= 25, `${name} did not paginate rows`);
        await evaluate(`document.querySelector('app-table-pagination button[aria-label="หน้าถัดไป"]')?.click()`);
        await until(() => evaluate("Array.from(document.querySelectorAll('.page-range')).some(e=> /แสดง (16|26|51)–/.test(e.textContent))"), name + ' next page');
        await evaluate("document.querySelector('app-table-pagination').scrollIntoView({block:'center'}); document.querySelector('app-table-pagination .select-trigger-container').click()");
        await until(() => evaluate("!!document.querySelector('.cdk-overlay-pane [role=option]')"), name + ' page-size menu');
        const option = await evaluate(`(()=>{const e=document.querySelector('.cdk-overlay-pane [role=option][title="10 แถว"]');const r=e.getBoundingClientRect();return {x:r.x+r.width/2,y:r.y+r.height/2,visible:!!document.elementFromPoint(r.x+r.width/2,r.y+r.height/2)?.closest('[role=option]')};})()`);
        assert(option.visible, `${name} page-size menu is clipped at ${width}px`);
        await send('Input.dispatchMouseEvent', { type: 'mousePressed', x: option.x, y: option.y, button: 'left', clickCount: 1 });
        await send('Input.dispatchMouseEvent', { type: 'mouseReleased', x: option.x, y: option.y, button: 'left', clickCount: 1 });
        await until(() => evaluate("document.querySelector('.page-range')?.textContent.includes('แสดง 1–10')"), name + ' page size reset');
        const screenshot = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
        fs.writeFileSync(path.join(output, `${name}-${width}.png`), Buffer.from(screenshot.data, 'base64'));
        results.push({ name, width, ...state });
        console.log(`${name} ${width}px: pagination and layout passed`);
      }
      const previousLoad = completedLoads;
      await send('Page.navigate', { url: origin + '/master-data?tab=grades' });
      await until(() => completedLoads > previousLoad, 'short table document load');
      await until(() => evaluate("(()=>{const e=document.querySelector('app-master-data-hub');return !!e && !!window.ng?.getComponent(e)})()"), 'short table load');
      await until(() => evaluate("document.querySelector('.page-range')?.textContent.includes('จาก 61')"), 'short table data');
      await evaluate("(()=>{const c=ng.getComponent(document.querySelector('app-master-data-hub'));c.masterDataService.conditionGrades.set([]);ng.applyChanges(c);})()");
      await until(() => evaluate("document.querySelector('.page-range')?.textContent.includes('แสดง 0–0 จาก 0')"), 'empty table');
      await evaluate("document.querySelector('app-table-pagination').scrollIntoView({block:'center'});document.querySelector('app-table-pagination .select-trigger-container').click()");
      await until(() => evaluate("!!document.querySelector('.cdk-overlay-pane [role=option]')"), 'empty table menu');
      assert(await evaluate("(()=>{const e=document.querySelector('.cdk-overlay-pane [role=option]');const r=e.getBoundingClientRect();return !!document.elementFromPoint(r.x+r.width/2,r.y+r.height/2)?.closest('[role=option]');})()"), 'Empty table page-size menu is clipped');
      const emptyScreenshot = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
      fs.writeFileSync(path.join(output, `empty-menu-${width}.png`), Buffer.from(emptyScreenshot.data, 'base64'));
      console.log(`empty table ${width}px: page-size menu is visible`);
    }
    assert.equal(missing.length, 0, missing.join('\n'));
    assert.equal(errors.length, 0, errors.join('\n'));
    fs.writeFileSync(path.join(output, 'results.json'), JSON.stringify(results, null, 2));
    console.log(`Checked ${results.length} menu views. Screenshots: ${output}`);
  } finally {
    if (ws) ws.close();
    browser.kill();
    server.close();
    await sleep(600);
    const resolved = path.resolve(profile);
    if (path.dirname(resolved) === path.resolve(os.tmpdir()) && path.basename(resolved).startsWith('mue2-table-browser-')) {
      try { fs.rmSync(resolved, { recursive: true, force: true, maxRetries: 3 }); } catch { /* Browser may still be closing its temporary profile. */ }
    }
  }
})().catch(error => { console.error(error); process.exitCode = 1; });
