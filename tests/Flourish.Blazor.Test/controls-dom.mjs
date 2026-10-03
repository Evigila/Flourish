import fs from 'node:fs/promises';
import assert from 'node:assert/strict';

let count=0;
const check=(name,action)=>{ action();count++;process.stdout.write(`PASS ${name}\n`); };
class Style {
  values=new Map();
  getPropertyValue(name){ return this.values.get(name)??''; }
  getPropertyPriority(){ return ''; }
  setProperty(name,value){ this.values.set(name,value); }
  removeProperty(name){ this.values.delete(name); }
}
class Node {
  attrs=new Map(); events=new Map(); style=new Style(); isConnected=true; disabled=false;
  hidden=false; children=[]; id=''; rect={left:20,right:80,top:100,bottom:136,width:60,height:36};
  classList={add(){},remove(){}};
  addEventListener(type,fn){ const list=this.events.get(type)??[];list.push(fn);this.events.set(type,list); }
  removeEventListener(type,fn){ this.events.set(type,(this.events.get(type)??[]).filter(f=>f!==fn)); }
  emit(type,event){ for(const fn of [...(this.events.get(type)??[])])fn(event); }
  setAttribute(name,value){ this.attrs.set(name,value);if(name==='open')this.open=true; }
  getAttribute(name){ return this.attrs.get(name)??null; }
  removeAttribute(name){ this.attrs.delete(name);if(name==='open')this.open=false; }
  getClientRects(){ return this.hidden?[]:[this.rect]; }
  getBoundingClientRect(){ return this.rect; }
  contains(node){ return node===this||this.children.includes(node); }
  closest(){ return null; }
  matches(selector){ return selector===':popover-open'?this.popoverOpen===true:true; }
  querySelectorAll(){ return this.children; }
  querySelector(){ return null; }
  focus(){ document.activeElement=this; }
  scrollIntoView(){ this.scrolled=true; }
  scrollTo(){}
  before(){}
  remove(){ this.isConnected=false;document.body.children=document.body.children.filter(item=>item!==this); }
  showPopover(){ this.popoverOpen=true; }
  hidePopover(){ this.popoverOpen=false; }
  showModal(){ this.open=true; }
  close(){ this.open=false; }
}
globalThis.document=new Node();
document.body=new Node();document.documentElement={clientWidth:800};document.activeElement=null;
document.body.append=function(...nodes){this.children.push(...nodes);};
document.createComment=()=>({isConnected:true,replaceWith(){}});
document.createElement=()=>new Node();
globalThis.window=new Node();window.innerHeight=600;
globalThis.MutationObserver=class{observe(){}disconnect(){this.disconnected=true;}};
globalThis.getComputedStyle=node=>{
  const keys=[...new Set([...node.style.values.keys(),...Object.keys(node.computedTheme??{})])];
  return Object.assign({length:keys.length,getPropertyValue:name=>node.style.getPropertyValue(name)||node.computedTheme?.[name]||''},keys);
};
globalThis.CSS={escape:value=>value};
globalThis.HTMLInputElement=Node;
globalThis.requestAnimationFrame=fn=>{fn();return 1;};globalThis.cancelAnimationFrame=()=>{};
const code=await fs.readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/controls.js',import.meta.url),'utf8');
const api=await import('data:text/javascript;base64,'+Buffer.from(code).toString('base64'));
const event=key=>({key,defaultPrevented:false,preventDefault(){this.defaultPrevented=true;},stopPropagation(){}});
const trigger=new Node();const panel=new Node();const first=new Node(),second=new Node();panel.children=[first,second];
panel.rect={width:200,height:120};trigger.style.setProperty('--f-danger','#ffb4ab');trigger.style.setProperty('--f-body-size','17px');
api.attachMenu(trigger,panel);
check('Menu opens in top layer with scoped danger theme',()=>{api.toggleMenu(trigger,panel);assert.equal(panel.popoverOpen,true);assert.equal(panel.style.getPropertyValue('--f-danger'),'#ffb4ab');assert.equal(document.activeElement,first);});
check('Menu arrow and End navigate enabled items',()=>{document.emit('keydown',event('ArrowDown'));assert.equal(document.activeElement,second);document.emit('keydown',event('Home'));assert.equal(document.activeElement,first);document.emit('keydown',event('End'));assert.equal(document.activeElement,second);});
check('Menu Escape closes, returns trigger and restores inline theme',()=>{const escape=event('Escape');document.emit('keydown',escape);assert.equal(escape.defaultPrevented,true);assert.equal(panel.popoverOpen,false);assert.equal(document.activeElement,trigger);assert.equal(panel.style.getPropertyValue('--f-danger'),'');});
check('Disabled menu trigger cannot reopen',()=>{trigger.disabled=true;api.toggleMenu(trigger,panel);assert.equal(panel.popoverOpen,false);trigger.disabled=false;});
check('Outside click closes without stealing focus',()=>{api.toggleMenu(trigger,panel);const outside=new Node();document.activeElement=outside;document.emit('pointerdown',{target:outside});assert.equal(panel.popoverOpen,false);assert.equal(document.activeElement,outside);});
check('Menu disposal removes component listeners',()=>{api.detachMenu(trigger,panel);assert.equal(trigger.events.get('keydown').length,0);});

const opener=new Node(),dialog=new Node(),close=new Node(),last=new Node();dialog.id='dialog-test';dialog.children=[close,last];
let closeRequests=0;
const reference={async invokeMethodAsync(name){assert.equal(name,'RequestCloseAsync');closeRequests++;}};
document.activeElement=opener;api.synchronizeDialog(dialog,true,reference);
check('Modal opens and contains keyboard focus',()=>{assert.equal(dialog.open,true);assert.equal(document.activeElement,close);document.activeElement=last;dialog.emit('keydown',event('Tab'));assert.equal(document.activeElement,close);const shift=event('Tab');shift.shiftKey=true;dialog.emit('keydown',shift);assert.equal(document.activeElement,last);});
check('Busy modal prevents native Escape without server close',()=>{dialog.setAttribute('aria-busy','true');const cancel=event();dialog.emit('cancel',cancel);assert.equal(cancel.defaultPrevented,true);assert.equal(closeRequests,0);assert.equal(dialog.open,true);});
dialog.setAttribute('aria-busy','false');const cancel=event();dialog.emit('cancel',cancel);await Promise.resolve();
check('Idle Escape awaits controlled close bridge',()=>{assert.equal(closeRequests,1);assert.equal(dialog.open,true);});
check('Controlled close returns focus',()=>{api.synchronizeDialog(dialog,false,reference);assert.equal(dialog.open,false);assert.equal(document.activeElement,opener);});
check('Modal disposal removes native handlers',()=>{api.detachDialog(dialog);assert.equal(dialog.events.get('cancel').length,0);assert.equal(dialog.events.get('keydown').length,0);});

check('Validation skips disabled or invisible errors',()=>{const root=new Node(),disabled=new Node(),hidden=new Node(),invalid=new Node();disabled.disabled=true;hidden.hidden=true;root.children=[disabled,hidden,invalid];assert.equal(api.focusFirstInvalid(root),true);assert.equal(document.activeElement,invalid);assert.equal(invalid.scrolled,true);});
const fallbackMenuTrigger=new Node(),fallbackMenu=new Node();fallbackMenu.showPopover=undefined;fallbackMenu.children=[new Node()];fallbackMenu.rect={width:190,height:80};fallbackMenuTrigger.computedTheme={'--f-danger':'#ffb4ab','color-scheme':'dark'};
check('Older-browser menu portal retains scope theme',()=>{api.toggleMenu(fallbackMenuTrigger,fallbackMenu);assert.equal(document.body.children.includes(fallbackMenu),true);assert.equal(fallbackMenu.style.getPropertyValue('color-scheme'),'dark');api.closeMenu(fallbackMenuTrigger,fallbackMenu);assert.equal(fallbackMenu.style.getPropertyValue('--f-danger'),'');api.detachMenu(fallbackMenuTrigger,fallbackMenu);});
const fallbackDialog=new Node(),background=new Node(),returnButton=new Node();fallbackDialog.showModal=undefined;fallbackDialog.id='fallback';fallbackDialog.children=[new Node()];fallbackDialog.computedTheme={'--f-danger':'#ffb4ab','--f-body-size':'17px','color-scheme':'dark'};background.inert=false;document.body.children=[background];document.activeElement=returnButton;
check('Older-browser modal copies theme and inerts background',()=>{api.synchronizeDialog(fallbackDialog,true,reference);assert.equal(fallbackDialog.open,true);assert.equal(background.inert,true);assert.equal(fallbackDialog.style.getPropertyValue('--f-danger'),'#ffb4ab');assert.equal(fallbackDialog.style.getPropertyValue('color-scheme'),'dark');});
check('Older-browser modal close restores inert flags, focus and theme',()=>{api.synchronizeDialog(fallbackDialog,false,reference);assert.equal(background.inert,false);assert.equal(fallbackDialog.style.getPropertyValue('--f-danger'),'');assert.equal(document.activeElement,returnButton);api.detachDialog(fallbackDialog);});
process.stdout.write(`${count}/${count} mocked DOM checks passed.\n`);
