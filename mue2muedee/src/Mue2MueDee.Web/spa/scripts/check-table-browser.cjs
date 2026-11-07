// Real-browser layout checks against a local mock API. No request reaches a shop database.
const fs = require('node:fs');
const path = require('node:path');
const os = require('node:os');
const http = require('node:http');
const { spawn } = require('node:child_process');
const assert = require('node:assert/strict');
const dist = path.resolve(__dirname, '../dist/frontend/browser');
const output = path.resolve(__dirname, '../.angular/table-ui-check');
const selectedView = process.argv[2] || process.env.TABLE_VIEW;
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
  status: 3, statusName: 'พร้อมขาย', isActive: true, role: 'Owner', userCount: 12, totalUsersCount: 12, productCount: 25, sourcedItemCount: 4, inventoryCount: 5, sortOrder: index + 1,
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
  const mutations = [];
  const server = http.createServer((req, res) => {
    const url = new URL(req.url, 'http://localhost');
    if (url.pathname.startsWith('/api/')) {
      if (url.pathname === '/api/__notification-test') {
        const status = Number(url.searchParams.get('status') || 200);
        const message = status === 200 ? 'บันทึกข้อมูลสำเร็จ' : status === 409
          ? 'ไม่สามารถทำรายการได้ เนื่องจากสินค้านี้ถูกขายหรือมีการเปลี่ยนแปลงสถานะ กรุณาตรวจสอบข้อมูลก่อนทำรายการอีกครั้ง'
          : 'ระบบไม่สามารถบันทึกข้อมูลได้ กรุณาลองใหม่อีกครั้ง';
        setTimeout(() => {
          res.writeHead(status, { 'Content-Type': 'application/json' });
          res.end(JSON.stringify({ success: status === 200, message, data: {} }));
        }, 350);
        return;
      }
      if (url.pathname === '/api/__loading-test') {
        const status = Number(url.searchParams.get('status') || 200);
        setTimeout(() => {
          res.writeHead(status, { 'Content-Type': 'application/json' });
          res.end(JSON.stringify({ success: status === 200, data: {}, message: 'Loading test' }));
        }, Number(url.searchParams.get('delay') || 1000));
        return;
      }
      // All mutations are blocked by this mock server.
      if (req.method !== 'GET') { mutations.push(req.url); res.writeHead(405).end(); return; }
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
    const checkActionCancellation = async (name, width, trigger) => {
      const before = mutations.length;
      await evaluate(trigger);
      await until(() => evaluate("!!document.querySelector('app-action-dialog .dialog-cancel')"), name + ' confirmation');
      await sleep(100);
      const cancel = await evaluate(`(()=>{const e=document.querySelector('.dialog-cancel');const r=e.getBoundingClientRect();return {x:r.x+r.width/2,y:r.y+r.height/2,visible:!!document.elementFromPoint(r.x+r.width/2,r.y+r.height/2)?.closest('.dialog-cancel'),fits:document.documentElement.scrollWidth<=innerWidth+2};})()`);
      assert(cancel.visible && cancel.fits, name + ' confirmation is clipped or behind another modal');
      const screenshot = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
      fs.writeFileSync(path.join(output, `action-${name}-${width}.png`), Buffer.from(screenshot.data, 'base64'));
      await send('Input.dispatchMouseEvent', { type: 'mousePressed', x: cancel.x, y: cancel.y, button: 'left', clickCount: 1 });
      await send('Input.dispatchMouseEvent', { type: 'mouseReleased', x: cancel.x, y: cancel.y, button: 'left', clickCount: 1 });
      await until(() => evaluate("!document.querySelector('app-action-dialog')"), name + ' cancellation');
      await sleep(100);
      assert.equal(mutations.length, before, name + ' sent a request despite cancellation');
      console.log(`${name} ${width}px: action modal cancellation passed`);
    };
    const checkGlobalLoading = async width => {
      await evaluate("document.getElementById('btn-open-create-store-modal').scrollIntoView({block:'center'})");
      await evaluate("document.querySelector('table.stores-table tbody tr button').click()");
      await until(() => evaluate("!!document.querySelector('app-action-dialog')"), 'underlying action dialog');
      const before = mutations.length;
      await evaluate(`(()=>{
        window.__loadingResult={finished:0,errors:0};
        const http=ng.getComponent(document.querySelector('app-super-admin-stores')).adminService.http;
        for(const url of ['/api/__loading-test?delay=800','/api/__loading-test?delay=2200&status=500']) {
          http.get(url).subscribe({next:()=>window.__loadingResult.finished++,error:()=>window.__loadingResult.errors++});
        }
      })()`);
      await until(() => evaluate("!!document.querySelector('.loading-card')"), 'global loading overlay');
      const state = await evaluate(`(()=>{
        const button=document.getElementById('btn-open-create-store-modal');const r=button.getBoundingClientRect();
        const hit=document.elementFromPoint(r.x+r.width/2,r.y+r.height/2);
        return {inert:document.querySelector('.application-content').inert,x:r.x+r.width/2,y:r.y+r.height/2,blocked:!hit?.closest('#btn-open-create-store-modal'),scroll:scrollY};
      })()`);
      assert(state.inert && state.blocked, 'Global loading did not block page controls');
      await send('Input.dispatchMouseEvent', { type: 'mousePressed', x: state.x, y: state.y, button: 'left', clickCount: 1 });
      await send('Input.dispatchMouseEvent', { type: 'mouseReleased', x: state.x, y: state.y, button: 'left', clickCount: 1 });
      await send('Input.dispatchKeyEvent', { type: 'keyDown', key: 'Escape', code: 'Escape', windowsVirtualKeyCode: 27 });
      await send('Input.dispatchKeyEvent', { type: 'keyUp', key: 'Escape', code: 'Escape', windowsVirtualKeyCode: 27 });
      await send('Input.dispatchKeyEvent', { type: 'keyDown', key: 'Tab', code: 'Tab', windowsVirtualKeyCode: 9 });
      await send('Input.dispatchKeyEvent', { type: 'keyUp', key: 'Tab', code: 'Tab', windowsVirtualKeyCode: 9 });
      assert(await evaluate("!!document.activeElement?.closest('.global-loading-panel')"), 'Loading did not keep keyboard focus inside the overlay');
      await send('Input.dispatchMouseEvent', { type: 'mouseWheel', x: 100, y: 100, deltaX: 0, deltaY: 500 });
      assert(await evaluate(`!!document.querySelector('.loading-card') && !!document.querySelector('app-action-dialog') && !document.querySelector('.store-modal-panel') && scrollY===${state.scroll}`), 'Loading allowed clicking, closing a dialog or scrolling');
      const screenshot = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
      fs.writeFileSync(path.join(output, `global-loading-${width}.png`), Buffer.from(screenshot.data, 'base64'));
      await until(() => evaluate("window.__loadingResult.finished===1"), 'first concurrent request');
      assert(await evaluate("!!document.querySelector('.loading-card') && document.querySelector('.application-content').inert"), 'Loading ended before all requests completed');
      await until(() => evaluate("window.__loadingResult.errors===1 && !document.querySelector('.loading-card')"), 'failed final request unlocks screen');
      assert(await evaluate("!document.querySelector('.application-content').inert && !!document.querySelector('app-action-dialog')"), 'Screen did not recover after API error');
      await evaluate("document.querySelector('.dialog-cancel').click()");
      await until(() => evaluate("!document.querySelector('app-action-dialog')"), 'original dialog cancellation');
      assert.equal(mutations.length, before, 'Loading allowed a store action to be sent');
      console.log(`global loading ${width}px: concurrent requests, blocked controls and error recovery passed`);
    };
    const checkNotifications = async width => {
      await evaluate("(()=>{const n=ng.getComponent(document.querySelector('app-notification-toast')).notifications;for(const item of n.items())n.dismiss(item.id);})()");
      for (const [status, kind] of [[200, 'success'], [409, 'warning'], [500, 'error'], [500, 'error']]) {
        await evaluate(`(()=>{const http=ng.getComponent(document.querySelector('app-super-admin-stores')).adminService.http;http.post('/api/__notification-test?status=${status}',{}).subscribe({error:()=>{}});})()`);
        await until(() => evaluate("!!document.querySelector('.loading-card')"), 'notification waits for loading');
        assert(await evaluate("document.querySelector('.toast-region').hidden"), 'Notification was shown behind Loading');
        await until(() => evaluate(`!document.querySelector('.loading-card') && !!document.querySelector('.toast.${kind}')`), kind + ' notification');
      }
      assert.equal(await evaluate("document.querySelectorAll('.toast.error').length"), 1, 'Duplicate error notifications were shown');
      const layout = await evaluate(`(()=>{const r=document.querySelector('.toast-region').getBoundingClientRect();return {right:r.right,left:r.left,top:r.top,width:document.documentElement.scrollWidth,count:document.querySelectorAll('.toast').length};})()`);
      assert(layout.count === 3 && layout.left >= 0 && layout.right <= width && layout.right >= width - 24 && layout.top >= 8 && layout.width <= width + 2, 'Notifications are not positioned at the top right or overflow the screen');
      const screenshot = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
      fs.writeFileSync(path.join(output, `notifications-${width}.png`), Buffer.from(screenshot.data, 'base64'));
      await evaluate("document.querySelector('.toast.warning .toast-close').click()");
      await until(() => evaluate("!document.querySelector('.toast.warning')"), 'notification dismissal');
      console.log(`notifications ${width}px: success, warning, error, duplicate prevention and dismissal passed`);
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
    if (selectedView === 'validation') {
      const cases = [
        ['login', '/login', 'app-login', 'c.onSubmit()'],
        ['inventory-create', '/inventory/new', 'app-inventory-form', 'c.onSubmit()'],
        ...['brands', 'categories', 'grades', 'products', 'customers'].map(tab =>
          [`master-${tab}`, `/master-data?tab=${tab}`, 'app-master-data-hub', 'c.openCreateModal(); c.submitForm()']),
        ['customer-create', '/customers', 'app-customer-list', 'c.openCreate(); c.onSubmit()'],
        ['store-create', '/super-admin/stores', 'app-super-admin-stores', 'c.openCreateModal(); c.onSubmit()'],
        ['store-settings', '/settings', 'app-store-settings', "c.storeForm.controls.name.setValue(''); c.onSaveStore()"],
        ['store-user', '/settings', 'app-store-settings', "c.activeTab.set('users'); c.showAddUserModal.set(true); c.onAddUserSubmit()"],
        ['purchase-customer', '/purchase/new', 'app-purchase-wizard', 'c.goToStep(2)'],
        ['purchase-product', '/purchase/new', 'app-purchase-wizard', `c.step1Form.controls.customerId.setValue('${id(0)}'); c.currentStep.set(2); c.goToStep(3)`],
        ['purchase-price', '/purchase/new', 'app-purchase-wizard', `c.step1Form.controls.customerId.setValue('${id(0)}'); c.step2Form.patchValue({productId:'${id(0)}',serialNumber:'TEST',conditionGradeId:'${id(0)}'},{emitEvent:false}); c.step3Form.controls.expectedSalePrice.setValue(null); c.currentStep.set(3); c.createAndSubmitPO()`],
        ['purchase-payment', `/purchase/${id(0)}`, 'app-purchase-detail', 'c.openPayment(); c.paymentForm.controls.amount.setValue(0); c.submitPayment()'],
        ['sale-cancel', `/sales/${id(0)}`, 'app-sale-detail', 'c.openCancel(); c.submitCancel()'],
        ['pos-customer', '/sales/pos', 'app-pos', `c.cartItems.set([{inventoryItem:${JSON.stringify(rows[0])},unitPrice:1000,discount:0,subTotal:1000,grossProfit:250,marginPercentage:25,warrantyDays:30}]); c.customerForm.controls.customerName.setValue(''); c.submitSale()`],
        ['pos-payment', '/sales/pos', 'app-pos', `c.cartItems.set([{inventoryItem:${JSON.stringify(rows[0])},unitPrice:1000,discount:0,subTotal:1000,grossProfit:250,marginPercentage:25,warrantyDays:30}]); c.paymentForm.controls.amount.setValue(0); c.addPayment()`],
        ['serial-trace', '/sales/trace', 'app-serial-trace', 'c.searchTrace()'],
        ['inventory-cost', `/inventory/${id(0)}`, 'app-inventory-detail', 'c.openAddCost(); c.submitAddCostItem()'],
        ['inventory-price', `/inventory/${id(0)}`, 'app-inventory-detail', 'c.openUpdatePrice(); c.updatePriceForm.controls.newPrice.setValue(0); c.submitUpdatePrice()'],
        ['inventory-status', `/inventory/${id(0)}`, 'app-inventory-detail', "c.openChangeStatus(); c.statusForm.controls.newStatus.setValue(''); c.submitChangeStatus()"]
      ];
      for (const width of [1440, 390]) {
        await send('Emulation.setDeviceMetricsOverride', { width, height: 960, deviceScaleFactor: 1, mobile: width < 600 });
        for (const [name, route, selector, action] of cases) {
          const previousLoad = completedLoads;
          await send('Page.navigate', { url: origin + route });
          await until(() => completedLoads > previousLoad, name + ' document');
          await until(() => evaluate(`!!window.ng && !!document.querySelector('${selector}') && !!ng.getComponent(document.querySelector('${selector}'))`), name + ' component');
          await until(() => evaluate("!document.querySelector('.loading-card')"), name + ' data');
          await sleep(100);
          const before = mutations.length;
          await evaluate(`(()=>{const c=ng.getComponent(document.querySelector('${selector}')); ${action}; ng.applyChanges(c);})()`);
          await until(() => evaluate("!!document.querySelector('.field-invalid')"), name + ' field validation');
          // Wait for the field's border-color transition before reading computed styles.
          await sleep(350);
          const fields = await evaluate(`(()=>{
            return [...document.querySelectorAll('.field-invalid')].filter(e=>e.getClientRects().length).map(e=>{
              const input=e.querySelector('[aria-invalid=true]');
              const error=e.querySelector('[role=status]');
              const border=e.querySelector('.select-trigger-container,.datepicker-trigger-container')||input;
              return {tag:e.tagName, borderElement:border?.className, message:error?.textContent.trim(), linked:!!input&&input.getAttribute('aria-describedby')===error?.id,
                border: border&&getComputedStyle(border).borderColor, below:!!input&&!!error&&error.getBoundingClientRect().top>=input.getBoundingClientRect().bottom-2};
            });
          })()`);
          assert(fields.length > 0, name + ' has no visible invalid fields');
          for (const field of fields) {
            assert(field.message && field.linked && field.below, name + ' missing linked error below field: ' + JSON.stringify(field));
            assert.equal(field.border, 'rgb(244, 63, 94)', name + ' missing red border: ' + JSON.stringify(field));
          }
          assert.equal(mutations.length, before, name + ' sent an invalid form to the API');
          assert(await evaluate('document.documentElement.scrollWidth<=innerWidth+2'), name + ' overflow');
          const screenshot = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
          fs.writeFileSync(path.join(output, `validation-${name}-${width}.png`), Buffer.from(screenshot.data, 'base64'));
          console.log(`validation ${name} ${width}px: ${fields.length} field errors and red borders passed`);
          results.push({ name, width, fields });
        }
      }
      assert.equal(errors.length, 0, errors.join('\n'));
      assert.equal(missing.length, 0, missing.join('\n'));
      fs.writeFileSync(path.join(output, 'validation-results.json'), JSON.stringify(results, null, 2));
      return;
    }
    for (const width of [1440, 390]) {
      await send('Emulation.setDeviceMetricsOverride', { width, height: 960, deviceScaleFactor: 1, mobile: width < 600 });
      for (const [name, route, selector, action] of views.filter(([name]) => !selectedView || selectedView.split(',').includes(name))) {
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
        await until(() => evaluate("!document.querySelector('.loading-card')"), name + ' loading finished');
        const state = await evaluate(`(()=>({ width:innerWidth, documentWidth:document.documentElement.scrollWidth, tables:[...document.querySelectorAll('table.app-table')].filter(t=>t.getClientRects().length).map(t=>({rows:t.querySelectorAll('tbody tr').length,scroll:t.parentElement.scrollWidth>t.parentElement.clientWidth})),ranges:[...document.querySelectorAll('.page-range')].map(e=>e.textContent.trim()) }))()`);
        assert(state.documentWidth <= width + 2, `${name} overflows viewport at ${width}: ${state.documentWidth}`);
        for (const table of state.tables) assert(table.rows <= 25, `${name} did not paginate rows`);
        if (name === 'purchase') {
          const layout = await evaluate(`(()=>{
            const customer=document.querySelector('td.purchase-customer');
            const items=document.querySelector('td.purchase-items');
            return {customerWidth:customer.getBoundingClientRect().width,itemsWidth:items.getBoundingClientRect().width,
              previewWraps:getComputedStyle(items.querySelector('.items-preview')).whiteSpace!=='nowrap'};
          })()`);
          assert(layout.customerWidth >= 238, 'Purchase customer column is squeezed');
          assert(layout.itemsWidth >= 270 && layout.previewWraps, 'Purchase item details are clipped');
          await evaluate("document.querySelector('table.data-table').scrollIntoView({block:'start'})");
          const tableScreenshot = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
          fs.writeFileSync(path.join(output, `purchase-columns-${width}.png`), Buffer.from(tableScreenshot.data, 'base64'));
        }
        if (name === 'stores') {
          const layout = await evaluate(`(()=>{
            const store=document.querySelector('td.store-column');
            const owner=document.querySelector('td.owner-column');
            const username=owner.querySelector('.owner-username').getBoundingClientRect();
            const email=owner.querySelector('.owner-email').getBoundingClientRect();
            return {storeWidth:store.getBoundingClientRect().width,ownerWidth:owner.getBoundingClientRect().width,separateLines:email.top>=username.bottom};
          })()`);
          assert(layout.storeWidth >= 270, 'Store name column is squeezed');
          assert(layout.ownerWidth >= 254, 'Store owner column is squeezed');
          assert(layout.separateLines, 'Owner username and email overlap');
          await evaluate("document.querySelector('table.stores-table').scrollIntoView({block:'start'})");
          const tableScreenshot = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
          fs.writeFileSync(path.join(output, `stores-columns-${width}.png`), Buffer.from(tableScreenshot.data, 'base64'));
        }
        if (name === 'inventory') {
          const layout = await evaluate(`(()=>{
            const cell=document.querySelector('td.inventory-product');
            const name=cell.querySelector('.model-name');
            return {cellWidth:cell.getBoundingClientRect().width,nameWidth:name.getBoundingClientRect().width};
          })()`);
          assert(layout.cellWidth >= 270, `Inventory product column is squeezed: ${layout.cellWidth}px`);
          assert(layout.nameWidth >= 190, `Inventory product name is squeezed: ${layout.nameWidth}px`);
          await evaluate("document.querySelector('table.inventory-table').scrollIntoView({block:'start'})");
          const tableScreenshot = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
          fs.writeFileSync(path.join(output, `inventory-columns-${width}.png`), Buffer.from(tableScreenshot.data, 'base64'));
        }
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
        if (name === 'purchase-detail') {
          await evaluate("(()=>{const c=ng.getComponent(document.querySelector('app-purchase-detail'));c.purchaseService.selectedOrder.update(po=>({...po,status:4,payments:[]}));ng.applyChanges(c);})()");
          await evaluate("document.querySelector('.action-buttons .btn-primary').click()");
          await until(() => evaluate("!!document.querySelector('.modal-dialog')"), 'purchase payment dialog');
          assert(await evaluate("(()=>{const r=document.querySelector('.modal-dialog').getBoundingClientRect();return r.top>=0 && r.bottom<=innerHeight && document.documentElement.scrollWidth<=innerWidth+2;})()"), 'Purchase payment dialog is clipped');
          const modalScreenshot = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
          fs.writeFileSync(path.join(output, `purchase-payment-${width}.png`), Buffer.from(modalScreenshot.data, 'base64'));
          await evaluate("document.querySelector('.modal-close').click()");
          await checkActionCancellation('purchase-receive', width, "document.querySelector('.btn-receive').click()");
        }
        if (name === 'stores') {
          await evaluate("document.getElementById('btn-open-create-store-modal').click()");
          await until(() => evaluate("!!document.querySelector('.store-modal-panel')"), 'create store dialog');
          await sleep(300);
          assert(await evaluate(`(()=>{
            const panel=document.querySelector('.store-modal-panel');const r=panel.getBoundingClientRect();
            const password=panel.querySelector('input[type=password]');
            return r.top>=0 && r.bottom<=innerHeight && panel.scrollHeight>=panel.clientHeight && password.value==='';
          })()`), 'Create store dialog is clipped or has a predefined password');
          await evaluate("document.querySelector('.store-modal-panel').scrollTop=10000");
          const modalScreenshot = await send('Page.captureScreenshot', { format: 'png', captureBeyondViewport: false });
          fs.writeFileSync(path.join(output, `stores-modal-${width}.png`), Buffer.from(modalScreenshot.data, 'base64'));
          await evaluate("document.querySelector('[aria-label=\"ปิดหน้าต่างสร้างร้านค้า\"]').click()");
          await checkActionCancellation('store-status', width, "document.querySelector('table.stores-table tbody tr button').click()");
          await checkGlobalLoading(width);
          await checkNotifications(width);
        }
        if (name === 'inventory') {
          await evaluate("document.querySelector('table.inventory-table tbody tr button[title=\"พิมพ์ป้ายสติ๊กเกอร์บาร์โค้ด\"]').click()");
          await until(() => evaluate("!!document.querySelector('button[title=\"รีเซ็ตกลับเป็น 4 รูปแบบมาตรฐานของระบบ\"]')"), 'sticker designer');
          await checkActionCancellation('sticker-reset', width, "document.querySelector('button[title=\"รีเซ็ตกลับเป็น 4 รูปแบบมาตรฐานของระบบ\"]').click()");
        }
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
