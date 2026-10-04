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
  addEventListener(type,fn,options){ const list=this.events.get(type)??[];list.push(fn);this.events.set(type,list);(this.eventOptions??=new Map()).set(fn,options); }
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
const event=key=>({key,defaultPrevented:false,propagationStopped:false,preventDefault(){this.defaultPrevented=true;},stopPropagation(){this.propagationStopped=true;}});
const trigger=new Node();const panel=new Node();const first=new Node(),second=new Node();panel.children=[first,second];
panel.rect={width:200,height:120};trigger.style.setProperty('--f-danger','#ffb4ab');trigger.style.setProperty('--f-body-size','17px');
api.attachMenu(trigger,panel);
check('Menu opens in top layer with scoped danger theme',()=>{api.toggleMenu(trigger,panel);assert.equal(panel.popoverOpen,true);assert.equal(panel.style.getPropertyValue('--f-danger'),'#ffb4ab');assert.equal(document.activeElement,first);});
check('Menu arrow and End navigate enabled items',()=>{document.emit('keydown',event('ArrowDown'));assert.equal(document.activeElement,second);document.emit('keydown',event('Home'));assert.equal(document.activeElement,first);document.emit('keydown',event('End'));assert.equal(document.activeElement,second);});
check('Menu Escape closes, returns trigger and restores inline theme',()=>{const escape=event('Escape');document.emit('keydown',escape);assert.equal(escape.defaultPrevented,true);assert.equal(panel.popoverOpen,false);assert.equal(document.activeElement,trigger);assert.equal(panel.style.getPropertyValue('--f-danger'),'');});
check('Disabled menu trigger cannot reopen',()=>{trigger.disabled=true;api.toggleMenu(trigger,panel);assert.equal(panel.popoverOpen,false);trigger.disabled=false;});
check('Outside click closes without stealing focus',()=>{api.toggleMenu(trigger,panel);const outside=new Node();document.activeElement=outside;document.emit('pointerdown',{target:outside});assert.equal(panel.popoverOpen,false);assert.equal(document.activeElement,outside);});
check('Menu disposal removes component listeners',()=>{api.detachMenu(trigger,panel);assert.equal(trigger.events.get('keydown').length,0);});

const hoverTrigger=new Node(),hoverPanel=new Node(),hoverItem=new Node(),hoverOutside=new Node();
hoverPanel.children=[hoverItem];hoverPanel.rect={width:190,height:80};document.activeElement=hoverOutside;
api.attachMenu(hoverTrigger,hoverPanel,true);
check('Hover menu opens without stealing focus and has no pointer gap',()=>{hoverTrigger.emit('pointerenter',{pointerType:'mouse'});assert.equal(hoverPanel.popoverOpen,true);assert.equal(document.activeElement,hoverOutside);assert.equal(hoverPanel.style.top,`${hoverTrigger.rect.bottom}px`);assert.equal(hoverTrigger.getAttribute('aria-expanded'),'true');});
check('Hover trigger to popup transition remains open',()=>{hoverTrigger.emit('pointerleave',{pointerType:'mouse',relatedTarget:hoverItem});assert.equal(hoverPanel.popoverOpen,true);});
check('Hover popup closes immediately on leaving both regions',()=>{hoverPanel.emit('pointerleave',{pointerType:'mouse',relatedTarget:hoverOutside});assert.equal(hoverPanel.popoverOpen,false);assert.equal(hoverTrigger.getAttribute('aria-expanded'),'false');assert.equal(document.activeElement,hoverOutside);});
check('Hover trigger click does not flash an already hovered menu closed',()=>{hoverTrigger.emit('pointerenter',{pointerType:'mouse'});api.toggleMenu(hoverTrigger,hoverPanel,false);assert.equal(hoverPanel.popoverOpen,true);hoverTrigger.emit('pointerleave',{pointerType:'mouse',relatedTarget:hoverOutside});assert.equal(hoverPanel.popoverOpen,false);});
check('Hover menus support keyboard entry and Escape focus return',()=>{const down=event('ArrowDown');hoverTrigger.emit('keydown',down);assert.equal(down.propagationStopped,true);assert.equal(hoverPanel.popoverOpen,true);assert.equal(document.activeElement,hoverItem);document.emit('keydown',event('Escape'));assert.equal(hoverPanel.popoverOpen,false);assert.equal(document.activeElement,hoverTrigger);});
check('Hover touch fallback opens explicitly and outside press closes',()=>{hoverTrigger.emit('pointerenter',{pointerType:'touch'});assert.equal(hoverPanel.popoverOpen,false);api.toggleMenu(hoverTrigger,hoverPanel,false);assert.equal(hoverPanel.popoverOpen,true);document.emit('pointerdown',{target:hoverOutside});assert.equal(hoverPanel.popoverOpen,false);});
check('Disabled hover menus remain closed and mode changes clear open state',()=>{hoverTrigger.disabled=true;hoverTrigger.emit('pointerenter',{pointerType:'mouse'});assert.equal(hoverPanel.popoverOpen,false);hoverTrigger.disabled=false;hoverTrigger.emit('pointerenter',{pointerType:'mouse'});api.attachMenu(hoverTrigger,hoverPanel,false);assert.equal(hoverPanel.popoverOpen,false);hoverTrigger.emit('pointerenter',{pointerType:'mouse'});assert.equal(hoverPanel.popoverOpen,false);});
check('Hover menu refresh preserves aria state and disposal removes pointer listeners',()=>{api.attachMenu(hoverTrigger,hoverPanel,true);hoverTrigger.emit('pointerenter',{pointerType:'mouse'});hoverTrigger.setAttribute('aria-expanded','false');api.attachMenu(hoverTrigger,hoverPanel,true);assert.equal(hoverTrigger.getAttribute('aria-expanded'),'true');api.detachMenu(hoverTrigger,hoverPanel);assert.equal(hoverTrigger.events.get('pointerenter').length,0);assert.equal(hoverPanel.events.get('pointerleave').length,0);assert.equal(hoverPanel.popoverOpen,false);});

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
const chromeTrigger=new Node(),surfacePanel=new Node();
const lightRoleFrame={
  '--f-primary':'#153A32','--f-accent':'#16745F','--f-canvas':'#F3F5F5','--f-surface':'#FFFFFF','--f-text':'#112924','--f-muted':'#4F645D',
  '--f-display-board':'#E5E8EB','--f-preview-light':'#C9DFDA','--f-preview-dark':'#2F5049','--f-border':'#9FAEA9','--f-danger':'#9D322D',
  '--f-click-light':'#B5C9C4','--f-click-dark':'#2A4842'
};
chromeTrigger.computedTheme={...lightRoleFrame,'--f-surface-preview':'#C9DFDA','--f-target-preview':'#2F5049','--f-row-hover':'#2F5049',
  '--f-surface-click':'#B5C9C4','--f-target-click':'#2A4842','--f-row-active':'#2A4842'};
surfacePanel.style.setProperty('--f-target-preview','host-preview');surfacePanel.style.setProperty('--f-target-click','host-click');surfacePanel.style.setProperty('--f-danger','host-danger');
surfacePanel.children=[new Node()];surfacePanel.rect={width:190,height:80};
check('A popup copies all palette roles but normalizes chrome hover and deep-click colors to its surface',()=>{
  api.toggleMenu(chromeTrigger,surfacePanel);assert.equal(surfacePanel.popoverOpen,true);
  for(const [name,value] of Object.entries(lightRoleFrame))assert.equal(surfacePanel.style.getPropertyValue(name),value,`${name} was not copied`);
  assert.equal(surfacePanel.style.getPropertyValue('--f-target-preview'),'#C9DFDA');assert.equal(surfacePanel.style.getPropertyValue('--f-row-hover'),'#C9DFDA');
  assert.equal(surfacePanel.style.getPropertyValue('--f-target-click'),'#B5C9C4');assert.equal(surfacePanel.style.getPropertyValue('--f-row-active'),'#B5C9C4');
  api.attachMenu(chromeTrigger,surfacePanel);assert.equal(surfacePanel.style.getPropertyValue('--f-target-click'),'#B5C9C4');
  api.closeMenu(chromeTrigger,surfacePanel);assert.equal(surfacePanel.style.getPropertyValue('--f-target-preview'),'host-preview');assert.equal(surfacePanel.style.getPropertyValue('--f-target-click'),'host-click');assert.equal(surfacePanel.style.getPropertyValue('--f-danger'),'host-danger');
  for(const name of Object.keys(lightRoleFrame).filter(name=>name!=='--f-danger'))assert.equal(surfacePanel.style.getPropertyValue(name),'',`${name} was not restored`);
  assert.equal(surfacePanel.style.getPropertyValue('--f-row-hover'),'');assert.equal(surfacePanel.style.getPropertyValue('--f-row-active'),'');api.detachMenu(chromeTrigger,surfacePanel);
});
const fallbackDialog=new Node(),background=new Node(),returnButton=new Node();fallbackDialog.showModal=undefined;fallbackDialog.id='fallback';fallbackDialog.children=[new Node()];fallbackDialog.computedTheme={'--f-danger':'#ffb4ab','--f-body-size':'17px','color-scheme':'dark'};background.inert=false;document.body.children=[background];document.activeElement=returnButton;
check('Older-browser modal copies theme and inerts background',()=>{api.synchronizeDialog(fallbackDialog,true,reference);assert.equal(fallbackDialog.open,true);assert.equal(background.inert,true);assert.equal(fallbackDialog.style.getPropertyValue('--f-danger'),'#ffb4ab');assert.equal(fallbackDialog.style.getPropertyValue('color-scheme'),'dark');});
check('Older-browser modal close restores inert flags, focus and theme',()=>{api.synchronizeDialog(fallbackDialog,false,reference);assert.equal(background.inert,false);assert.equal(fallbackDialog.style.getPropertyValue('--f-danger'),'');assert.equal(document.activeElement,returnButton);api.detachDialog(fallbackDialog);});
check('Text selection keeps ordinary replacement but preserves masked carets',()=>{
  const input=new Node();input.type='text';input.value='AB-1234';input.dataset={};let selections=0;input.select=()=>selections++;
  document.activeElement=input;api.attachTextSelection(input);input.emit('focus',{});assert.equal(selections,1);
  input.dataset.inputMask='AA-0000';input.emit('focus',{});assert.equal(selections,1);
  input.dataset.inputMask='';input.emit('focus',{});assert.equal(selections,1);
  delete input.dataset.inputMask;input.dataset.preserveSelection='true';input.emit('focus',{});assert.equal(selections,1);
  api.detachTextSelection(input);assert.equal(input.events.get('focus').length,0);
});
const inputBehaviorCode=await fs.readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/primitives/input-behaviors.js',import.meta.url),'utf8');
await import('data:text/javascript;base64,'+Buffer.from(inputBehaviorCode).toString('base64'));
check('Primitive focus selection cannot reselect a masked input through its document listener',()=>{
  const input=new Node();input.type='text';input.value='AB-1234';input.dataset={inputMask:'AA-0000'};let selections=0;input.select=()=>selections++;
  document.activeElement=input;document.emit('focusin',{target:input});assert.equal(selections,0);
  delete input.dataset.inputMask;document.emit('focusin',{target:input});assert.equal(selections,1);
  input.readOnly=true;document.emit('focusin',{target:input});assert.equal(selections,1);
});
check('Mask formatting precedes bound event reads and keeps a normalized mid-text caret',()=>{
  const input=new Node();input.type='text';input.value='ab12';input.dataset={inputMask:'AA-0000'};input.selectionStart=4;
  input.setSelectionRange=(start,end)=>{input.selectionStart=start;input.selectionEnd=end;};
  const handler=document.events.get('input').at(-1);assert.equal(document.eventOptions.get(handler).capture,true);
  document.emit('input',{target:input});assert.equal(input.value,'AB-12');assert.equal(input.selectionStart,5);assert.equal(input.selectionEnd,5);
  input.value='AB-12x3';input.selectionStart=6;document.emit('input',{target:input});assert.equal(input.value,'AB-123');assert.equal(input.selectionStart,5);
});
process.stdout.write(`${count}/${count} mocked DOM checks passed.\n`);
