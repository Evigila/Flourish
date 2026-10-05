const assert = require('node:assert/strict');
const { chromium } = require(process.env.PLAYWRIGHT_MODULE || 'playwright');
const gallery = process.env.GALLERY_URL || 'http://127.0.0.1:5198';
const native = process.env.NATIVE_URL || 'http://127.0.0.1:5000';

(async () => {
  const browser = await chromium.launch({
    executablePath: process.env.BROWSER_EXECUTABLE || 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
    headless: true
  });
  const errors = [];
  const assetFailures = [];
  try {
    const english = await browser.newContext({ locale: 'en-US' });
    const chinese = await browser.newContext({ locale: 'zh-CN' });
    const a = await english.newPage();
    const b = await chinese.newPage();
    for (const page of [a, b]) {
      page.on('pageerror', error => errors.push(error.message));
      page.on('response', response => {
        if (response.status() >= 400 && /\/_content\/|\/_framework\//.test(response.url()))
          assetFailures.push(`${response.status()} ${response.url()}`);
      });
    }
    const [responseA, responseB] = await Promise.all([a.goto(gallery), b.goto(gallery)]);
    assert.equal(responseA.status(), 200);
    assert.equal(responseB.status(), 200);
    assert.match(await responseA.text(), /<html lang="en-US">/);
    assert.match(await responseB.text(), /<html lang="zh-CN">/);
    await Promise.all([a.waitForLoadState('networkidle'), b.waitForLoadState('networkidle')]);
    assert.equal(await a.locator('h1').innerText(), 'Home');
    assert.equal(await b.locator('h1').innerText(), '主页');

    // The library picker is the standard SelectBox, using its native change event.
    await a.locator('.f-titlebar-end select').selectOption('zh-CN');
    await a.waitForSelector('.f-root[lang="zh-CN"]');
    assert.equal(await a.locator('h1').innerText(), '主页');
    assert.equal(await a.title(), '主页 · Gallery');
    assert.equal(await a.locator('.f-primary-item[href="/framework"]').getAttribute('aria-label'), '框架');
    assert.equal(await a.locator('.f-menu-trigger').getAttribute('aria-label'), '页面');
    assert.equal(await a.locator('.f-display-board-copy').getAttribute('aria-label'), '复制');
    assert.equal(await b.locator('.f-root').getAttribute('lang'), 'zh-CN');
    await b.locator('.f-titlebar-end select').selectOption('en-US');
    await b.waitForSelector('.f-root[lang="en-US"]');
    assert.equal(await b.locator('h1').innerText(), 'Home');
    assert.equal(await a.locator('h1').innerText(), '主页');
    assert.equal((await english.cookies()).some(cookie => cookie.name === '.AspNetCore.Culture'), false);

    await a.locator('.f-primary-item[href="/framework"]').click();
    await a.locator('.f-secondary-item[href="/framework/localization"]').click();
    await a.waitForURL('**/framework/localization');
    await a.getByRole('heading', { level: 1, name: '语言与本地化', exact: true }).waitFor();
    assert.equal(await a.locator('h1').innerText(), '语言与本地化');
    await a.locator('#localization-format').selectOption('pt-BR');
    await a.waitForFunction(() => document.querySelector('.f-content-scroll')?.textContent.includes('12.345,67'));
    assert.equal(await a.locator('.f-data-display summary').innerText(), '显示');
    assert.match(await a.locator('.f-data-count').first().innerText(), /项 1-1 \/ 共 1/);
    assert.match(await a.locator('.f-data-table').innerText(), /12345,67/);
    await a.getByRole('button', { name: '打开对话框', exact: true }).click();
    await a.locator('dialog[open]').waitFor();
    assert.equal(await a.locator('dialog[open] .f-dialog-close').getAttribute('aria-label'), '关闭');
    await a.locator('dialog[open] .f-dialog-close').click();
    await a.locator('.f-titlebar-end select').selectOption('en-US');
    await a.waitForSelector('.f-root[lang="en-US"]');
    await a.getByRole('heading', { level: 1, name: 'Language and localization', exact: true }).waitFor();
    assert.equal(await a.locator('h1').innerText(), 'Language and localization');
    assert.equal(await a.locator('.f-data-display summary').innerText(), 'Display');
    assert.equal(await a.locator('#localization-format').inputValue(), 'en-US');
    assert.equal(await a.locator('.f-display-board-copy').first().getAttribute('aria-label'), 'Copy');
    await a.getByRole('button', { name: 'Open dialog', exact: true }).click();
    await a.locator('dialog[open]').waitFor();
    assert.equal(await a.locator('dialog[open] .f-dialog-close').getAttribute('aria-label'), 'Close');
    await a.locator('dialog[open] .f-dialog-close').click();

    // Language changes retain the expanded navigation branch and selected route.
    await a.locator('.f-primary-item[href="/controls"]').click();
    const branch = a.locator('.f-secondary-toggle[data-navigation-target="/controls/inputs"]');
    if (await branch.getAttribute('aria-expanded') !== 'true') await branch.click();
    await a.locator('.f-third-item[href="/controls/inputs/textboxsample"]').click();
    await a.waitForURL('**/controls/inputs/textboxsample');
    await a.waitForSelector('.f-third-item[href="/controls/inputs/textboxsample"][aria-current="page"]');
    await branch.click();
    await a.waitForSelector('.f-secondary-toggle[data-navigation-target="/controls/inputs"][aria-expanded="false"]');
    const panelId = await branch.getAttribute('aria-controls');
    assert.equal(await branch.getAttribute('aria-expanded'), 'false');
    await a.locator('.f-titlebar-end select').selectOption('zh-CN');
    await a.waitForSelector('.f-root[lang="zh-CN"]');
    assert.equal(await branch.getAttribute('aria-controls'), panelId);
    assert.equal(await branch.getAttribute('aria-expanded'), 'false');
    assert.match(await branch.getAttribute('aria-label'), /输入与选择/);
    assert.equal(await a.locator('.f-third-item[href="/controls/inputs/textboxsample"]').getAttribute('aria-current'), 'page');
    assert.equal(await a.locator('.f-primary-command').getAttribute('aria-label'), '切换主题');

    // Translated menu items still dispatch the existing command identity.
    await a.locator('.f-menu-trigger[aria-label="页面"]').hover();
    await a.getByRole('menuitem', { name: '案例', exact: true }).click();
    await a.waitForURL('**/examples');
    assert.equal(await b.locator('.f-root').getAttribute('lang'), 'en-US');
    await a.reload();
    await a.waitForLoadState('networkidle');
    assert.equal(await a.locator('.f-root').getAttribute('lang'), 'en-US');

    const nativeResponse = await english.request.get(native);
    assert.equal(nativeResponse.status(), 200);
    const nativeHtml = await nativeResponse.text();
    assert.match(nativeHtml, /Native controls/);
    assert.match(nativeHtml, /Skip to content/);
    assert.doesNotMatch(nativeHtml, /Arkheide\.Flourish\.Blazor\.Design|Essential\.Culture/);
    assert.deepEqual(errors, []);
    assert.deepEqual(assetFailures, []);
    console.log(JSON.stringify({ pass: true, checks: [
      'SSR negotiated English and Chinese', 'two independent live circuits', 'home and PageTitle refresh',
      'top bar primary secondary fixed labels', 'framework copy and dialog defaults',
      'separate Brazilian formatting and default table labels', 'third navigation state preservation',
      'translated menu command routing', 'reload restoration without cookie writes',
      'Framework-only Native SSR', 'no browser exceptions or failed framework assets'
    ] }));
  } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
