import fs from 'node:fs/promises';
import assert from 'node:assert/strict';
const root = new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/primitives/', import.meta.url);
const originCode = await fs.readFile(new URL('interaction-origin.js', root), 'utf8');
const originUrl = 'data:text/javascript;base64,' + Buffer.from(originCode).toString('base64');
const origin = await import(originUrl);
let count = 0;
function check(name, action) { action(); count++; console.log('PASS ' + name); }
const element = (inMenu = false) => ({ isConnected: true, closest: () => inMenu ? {} : null, focus() { document.activeElement = this; } });
globalThis.document = { body: element(), documentElement: element(), activeElement: null, querySelector: () => null };
const observers = [];
globalThis.MutationObserver = class {
  constructor() { observers.push(this); }
  observe() {}
  disconnect() { this.disconnected = true; }
};
globalThis.CSS = { escape: value => value };
const rowTrigger = element();
check('Transient row-menu invoker survives menu dismissal', () => { origin.rememberInvoker(rowTrigger); assert.equal(origin.resolveInvoker(document.body), rowTrigger); });
check('Connected ordinary dialog invoker takes precedence', () => { const direct = element(); origin.rememberInvoker(rowTrigger); assert.equal(origin.resolveInvoker(direct), direct); });
check('Removed menu item resolves to its row trigger once', () => { origin.rememberInvoker(rowTrigger); assert.equal(origin.resolveInvoker(element(true)), rowTrigger); assert.equal(origin.resolveInvoker(document.body), null); });
check('Disconnected row does not receive focus', () => { rowTrigger.isConnected = false; origin.rememberInvoker(rowTrigger); assert.equal(origin.resolveInvoker(document.body), null); rowTrigger.isConnected = true; });
const controlsCode = (await fs.readFile(new URL('../controls.js', root), 'utf8'))
  .replace(/(["'])\.\/primitives\/interaction-origin\.js\1/, JSON.stringify(originUrl));
const controls = await import('data:text/javascript;base64,' + Buffer.from(controlsCode).toString('base64'));
function dialogFixture(id) {
  return {
    open: false, isConnected: true, id, listeners: new Map(),
    addEventListener(type, listener) { this.listeners.set(type, listener); },
    removeEventListener(type, listener) { if (this.listeners.get(type) === listener) this.listeners.delete(type); },
    querySelector: () => null, querySelectorAll: () => [], getAttribute: () => null,
    showModal() { this.open = true; }, close() { this.open = false; },
    focus() { document.activeElement = this; }
  };
}
const dialog = dialogFixture('test-dialog');
origin.rememberInvoker(rowTrigger);
document.activeElement = document.body;
await Promise.resolve();
check('Current Dialog restores the originating row after an async menu action', () => {
  controls.synchronizeDialog(dialog, true, {});
  assert.equal(dialog.open, true); assert.equal(document.activeElement, dialog);
  controls.synchronizeDialog(dialog, false, {});
  assert.equal(dialog.open, false); assert.equal(document.activeElement, rowTrigger);
});
check('Current Dialog detaches its listeners and native observer', () => {
  controls.detachDialog(dialog);
  assert.equal(dialog.listeners.size, 0); assert.equal(observers[0].disconnected, true);
});
check('Ordinary Dialog opener takes precedence over a remembered menu action', () => {
  const direct = element(), current = dialogFixture('direct-dialog');
  origin.rememberInvoker(rowTrigger); document.activeElement = direct;
  controls.synchronizeDialog(current, true, {}); controls.detachDialog(current);
  assert.equal(document.activeElement, direct); assert.equal(current.open, false);
  assert.equal(current.listeners.size, 0);
});
check('Removed Dialog opener returns to its explicit aria-controls replacement', () => {
  const removed = element(), replacement = element(), current = dialogFixture('replacement-dialog');
  document.activeElement = removed; controls.synchronizeDialog(current, true, {});
  removed.isConnected = false;
  document.querySelector = selector => selector === '[aria-controls="replacement-dialog"]' ? replacement : null;
  controls.synchronizeDialog(current, false, {});
  assert.equal(document.activeElement, replacement); controls.detachDialog(current);
  document.querySelector = () => null;
});
console.log(count + ' interaction-origin checks passed.');
