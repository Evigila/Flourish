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
  attrs=new Map(); events=new Map(); style=new Style(); isConnected=true; disabled=false; dataset={};
  hidden=false; children=[]; id=''; rect={left:20,right:80,top:100,bottom:136,width:60,height:36};
  classList={add(){},remove(){},toggle(){}};
  addEventListener(type,fn,options){ const list=this.events.get(type)??[];list.push(fn);this.events.set(type,list);(this.eventOptions??=new Map()).set(fn,options); }
  removeEventListener(type,fn){ this.events.set(type,(this.events.get(type)??[]).filter(f=>f!==fn)); }
  emit(type,event){ for(const fn of [...(this.events.get(type)??[])])fn(event); }
  dispatchEvent(event){ if(!event.target)Object.defineProperty(event,'target',{value:this});this.emit(event.type,event); if(event.bubbles && !event.cancelBubble && !event.propagationStopped)this.parentElement?.dispatchEvent(event);return true; }
  setAttribute(name,value){ this.attrs.set(name,value);if(name==='open')this.open=true; }
  getAttribute(name){ return this.attrs.get(name)??null; }
  removeAttribute(name){ this.attrs.delete(name);if(name==='open')this.open=false; }
  getClientRects(){ return this.hidden?[]:[this.rect]; }
  getBoundingClientRect(){ const limits=this.style.getPropertyValue('max-height').match(/[\d.]+(?=px)/g)?.map(Number);return limits?.length?{...this.rect,height:Math.min(this.rect.height,...limits)}:this.rect; }
  contains(node){ return node===this||this.children.includes(node); }
  closest(){ return null; }
  matches(selector){ return selector===':popover-open'?this.popoverOpen===true:selector===':disabled'?this.disabled:true; }
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
document.createElement=tag=>{const node=new Node();if(tag==='canvas')node.getContext=()=>null;return node;};
globalThis.window=new Node();window.innerHeight=600;
globalThis.MutationObserver=class{observe(){}disconnect(){this.disconnected=true;}};
globalThis.getComputedStyle=node=>{
  const keys=[...new Set([...node.style.values.keys(),...Object.keys(node.computedTheme??{})])];
  return Object.assign({length:keys.length,maxHeight:node.style.getPropertyValue('max-height')||'none',getPropertyValue:name=>node.style.getPropertyValue(name)||node.computedTheme?.[name]||''},keys);
};
globalThis.CSS={escape:value=>value};
globalThis.HTMLInputElement=Node;
globalThis.requestAnimationFrame=fn=>{fn();return 1;};globalThis.cancelAnimationFrame=()=>{};
const code=await fs.readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/controls.js',import.meta.url),'utf8');
const originCode=await fs.readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/primitives/interaction-origin.js',import.meta.url),'utf8');
const originUrl='data:text/javascript;base64,'+Buffer.from(originCode).toString('base64');
const controlsUrl='data:text/javascript;base64,'+Buffer.from(code.replace(/(["'])\.\/primitives\/interaction-origin\.js\1/,JSON.stringify(originUrl))).toString('base64');
const api=await import(controlsUrl);
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

const wideHoverRoot=new Node(),anchoredTrigger=new Node(),anchoredPanel=new Node();
wideHoverRoot.rect={left:300,right:760,top:0,bottom:136,width:460,height:136};
anchoredTrigger.rect={left:610,right:700,top:40,bottom:88,width:90,height:48};
anchoredTrigger.closest=selector=>selector==='.f-action-menu'?wideHoverRoot:null;
wideHoverRoot.children=[anchoredTrigger,anchoredPanel];anchoredPanel.children=[new Node()];anchoredPanel.rect={width:180,height:140};
api.attachMenu(anchoredTrigger,anchoredPanel,true);
check('Hover popup anchors to the button rather than a stretched pointer region',()=>{
  wideHoverRoot.emit('pointerenter',{pointerType:'mouse'});assert.equal(anchoredPanel.style.left,'520px');assert.equal(anchoredPanel.style.top,'88px');
});
check('Open hover popup follows its control through scroll and viewport resize',()=>{
  anchoredTrigger.rect={left:720,right:810,top:530,bottom:578,width:90,height:48};
  window.emit('resize',{});assert.equal(anchoredPanel.style.left,'608px');assert.equal(anchoredPanel.style.top,'390px');
  anchoredTrigger.rect={left:500,right:590,top:110,bottom:158,width:90,height:48};
  document.emit('scroll',{});assert.equal(anchoredPanel.style.left,'410px');assert.equal(anchoredPanel.style.top,'158px');api.detachMenu(anchoredTrigger,anchoredPanel);
});

const disclosure=new Node(),summary=new Node(),displayPanel=new Node(),disabledOption=new Node(),displayFirst=new Node(),displayLast=new Node();
disclosure.open=false;disclosure.children=[summary,displayPanel];summary.rect={left:700,right:790,top:550,bottom:586,width:90,height:36};
displayPanel.rect={width:200,height:160};disabledOption.disabled=true;displayPanel.children=[disabledOption,displayFirst,displayLast];
displayPanel.querySelectorAll=()=>displayPanel.children.filter(item=>!item.disabled);
displayPanel.style.setProperty('max-height','420px');
api.attachDisclosureMenu(disclosure,summary,displayPanel);
check('Native display disclosure uses shared top-layer positioning and flips above at the viewport bottom',()=>{
  disclosure.open=true;disclosure.emit('toggle',{});assert.equal(displayPanel.popoverOpen,true);assert.equal(summary.getAttribute('aria-expanded'),'true');
  assert.equal(displayPanel.style.left,'588px');assert.equal(displayPanel.style.top,'384px');
});
check('Display options keep native multi-selection open and keyboard navigation skips disabled controls',()=>{
  document.emit('pointerdown',{target:displayFirst});document.activeElement=displayFirst;document.emit('focusin',{target:displayFirst});
  assert.equal(disclosure.open,true);assert.equal(displayPanel.popoverOpen,true);
  document.emit('keydown',event('End'));assert.equal(document.activeElement,displayLast);
  document.emit('keydown',event('Home'));assert.equal(document.activeElement,displayFirst);
});
check('Display Escape restores summary focus and native open state while preserving prior height limits',()=>{
  document.emit('keydown',event('Escape'));assert.equal(disclosure.open,false);assert.equal(displayPanel.popoverOpen,false);
  assert.equal(document.activeElement,summary);assert.equal(summary.getAttribute('aria-expanded'),'false');assert.equal(displayPanel.style.getPropertyValue('max-height'),'420px');
});
check('Native summary collapse closes its shared popup without changing the current focus',()=>{
  disclosure.open=true;disclosure.emit('toggle',{});document.activeElement=summary;
  disclosure.open=false;disclosure.emit('toggle',{});assert.equal(displayPanel.popoverOpen,false);assert.equal(document.activeElement,summary);
  assert.equal(summary.getAttribute('aria-expanded'),'false');
});
check('Tall display popup scrolls within the larger available side and can expand again after scrolling',()=>{
  displayPanel.rect={width:200,height:900};summary.rect={left:700,right:790,top:260,bottom:308,width:90,height:48};
  summary.emit('keydown',event('ArrowDown'));assert.equal(disclosure.open,true);assert.equal(document.activeElement,displayFirst);
  assert.equal(displayPanel.style.top,'314px');assert.equal(displayPanel.style.getPropertyValue('max-height'),'min(274px, 420px)');
  summary.rect={left:700,right:790,top:530,bottom:578,width:90,height:48};document.emit('scroll',{});
  assert.equal(displayPanel.style.top,'104px');assert.equal(displayPanel.style.getPropertyValue('max-height'),'min(512px, 420px)');
});
check('Display outside interaction closes without taking focus and disposal removes native toggle listeners',()=>{
  const outside=new Node();document.activeElement=outside;document.emit('pointerdown',{target:outside});
  assert.equal(disclosure.open,false);assert.equal(displayPanel.popoverOpen,false);assert.equal(document.activeElement,outside);
  api.detachMenu(summary,displayPanel);assert.equal(disclosure.events.get('toggle').length,0);assert.equal(summary.events.get('keydown').length,0);
});

const dataCode=await fs.readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/data.js',import.meta.url),'utf8');
const selectionCode=await fs.readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/multi-select-box.js',import.meta.url),'utf8');
const selectionUrl='data:text/javascript;base64,'+Buffer.from(selectionCode.replace("'./controls.js'",JSON.stringify(controlsUrl))).toString('base64');
const selectionApi=await import(selectionUrl);
const dataApi=await import('data:text/javascript;base64,'+Buffer.from(dataCode.replace("'./multi-select-box.js'",JSON.stringify(selectionUrl))).toString('base64'));
check('Progressive directory searches declared visible text with culture-aware accents and column boundaries',()=>{
  const rows=[{index:0,cells:{name:'João',country:'Brasil'}},{index:1,cells:{name:'Ana',country:'Portugal'}}];
  assert.deepEqual(dataApi.processDirectory(rows,'JOAO','name',null,false,'pt-BR'),[rows[0]]);
  assert.deepEqual(dataApi.processDirectory(rows,'Portugal','name',null,false,'pt-BR'),[]);
  assert.deepEqual(dataApi.processDirectory(rows,'Portugal','',null,false,'pt-BR'),[rows[1]]);
});
check('Progressive directory sorting is stable presentation-text sorting and reset retains rendered order',()=>{
  const rows=[{index:0,cells:{name:'Água'}},{index:1,cells:{name:'agua'}},{index:2,cells:{name:'João'}}];
  assert.deepEqual(dataApi.processDirectory(rows,'','','name',true,'pt-BR').map(row=>row.index),[2,0,1]);
  assert.deepEqual(dataApi.processDirectory(rows,'','',null,false,'pt-BR'),rows);
});
function progressiveFixture(){
  const root=new Node(),table=new Node(),body=new Node(),scroll=new Node();
  root.dataset={fProgressive:'true',fInitialPageSize:'2',fCulture:'pt-BR',fItemsLabel:'Items',fTotalLabel:'Total',fRangeFormat:'{0} {1}-{2} / {3} {4}',fEmptyMessage:'None'};
  const rows=['João','Ana','Carla'].map((name,index)=>{
    const row=new Node(),cell=new Node();cell.dataset={fColumn:'name'};cell.querySelector=()=>({textContent:name});
    row.querySelectorAll=selector=>selector==='td[data-f-column]'?[cell]:[];row.index=index;return row;
  });
  body.children=[...rows];body.querySelectorAll=()=>rows;
  body.append=node=>{body.children=body.children.filter(child=>child!==node);body.children.push(node);};
  table.querySelector=selector=>selector==='tbody'?body:null;table.querySelectorAll=()=>[];table.closest=()=>scroll;
  scroll.after=node=>{root.empty=node;};
  const page=new Node(),size=new Node(),previous=new Node(),next=new Node(),counter=new Node(),label=new Node();
  page.dataset={};size.dataset={};counter.querySelector=()=>label;
  root.querySelector=selector=>selector==='[data-f-table]'?table:null;
  root.querySelectorAll=selector=>({'[data-f-search-column] option':[{value:'name'}],'[data-f-page]':[page],'[data-f-page-size]':[size],'[data-f-page-previous]':[previous],'[data-f-page-next]':[next],'.f-data-count':[counter]}[selector]??[]);
  root.classList={add(){},remove(){},toggle(name,value){root.cards=value;}};
  root.contains=node=>node===root||rows.includes(node);root.children=rows;
  const target=(marker,value)=>{const node=new Node();node.dataset={};node.value=value;node.hasAttribute=name=>node.attrs.has(name);node.matches=selector=>selector===`[${marker}]`;node.closest=selector=>selector.includes(`[${marker}]`)?node:null;node.setAttribute(marker,'');return node;};
  return{root,rows,body,page,label,next,query:target('data-f-search-query',''),view:target('data-f-view',''),sort:target('data-f-sort-key',''),nextTarget:target('data-f-page-next','')};
}
check('Progressive canonical directory pages searches sorts and switches cards on one retained DOM',()=>{
  const f=progressiveFixture();dataApi.synchronize(f.root,'progressive-owned');
  assert.deepEqual(f.rows.map(row=>row.hidden),[false,false,true]);assert.equal(f.label.textContent,'Items 1-2 / Total 3');
  f.root.emit('click',{...event(''),target:f.nextTarget});assert.deepEqual(f.rows.map(row=>row.hidden),[true,true,false]);assert.equal(f.page.value,2);
  f.query.value='joao';f.root.emit('input',{...event(''),target:f.query});assert.deepEqual(f.rows.map(row=>row.hidden),[false,true,true]);assert.equal(f.label.textContent,'Items 1-1 / Total 1');
  f.query.value='';f.root.emit('input',{...event(''),target:f.query});f.sort.dataset.fSortKey='name';f.root.emit('click',{...event(''),target:f.sort});
  assert.deepEqual(f.body.children.map(row=>row.index),[0,2,1]);
  f.root.emit('click',{...event(''),target:f.sort});assert.deepEqual(f.body.children.map(row=>row.index),[1,2,0]);
  f.root.emit('click',{...event(''),target:f.sort});assert.deepEqual(f.body.children.map(row=>row.index),[0,1,2]);
  f.view.dataset.fView='cards';f.root.emit('click',{...event(''),target:f.view});assert.equal(f.root.cards,true);
  assert.equal(new Set(f.body.children).size,3);assert.deepEqual([...f.rows].sort((a,b)=>a.index-b.index),f.rows);dataApi.detach('progressive-owned');
});
check('Progressive native POST action stays the same unique form after paging sorting and disposal',()=>{
  const f=progressiveFixture(),form=new Node(),submitter=new Node(),surface=new Node();form.tagName='FORM';submitter.tagName='BUTTON';submitter.matches=()=>false;form.querySelectorAll=()=>[submitter];form.requestSubmit=value=>{assert.equal(value,submitter);form.submitted=(form.submitted??0)+1;};
  f.rows[0].querySelectorAll=selector=>selector==='[data-record-open]'?[form]:selector==='td[data-f-column]'?[]:[];
  surface.closest=selector=>selector.includes('f-data-openable')?f.rows[0]:null;
  dataApi.synchronize(f.root,'progressive-post');f.root.emit('dblclick',{...event(''),target:surface});assert.equal(form.submitted,1);
  assert.equal(new Set(f.body.children).size,3);dataApi.detach('progressive-post');assert.equal(f.root.events.get('dblclick').length,0);assert.equal(f.root.events.get('input').length,0);assert.equal(f.root.events.get('change').length,0);
});
function selectionFixture(id,keys=[],isStatic=false) {
  const root=new Node(),summary=new Node(),panel=new Node(),caption=new Node(),snapshots=[];
  root.dataset={fSelectionInstance:id,fSelectionStatic:String(isStatic),fSelectionDisabled:'false',fReorderEnabled:'true',fMinimumSelected:'0',fMaximumSelections:'2147483647',fFiltering:'false',fCreating:'false',fSelectionEmpty:'None selected',fSelectionCount:'items selected'};
  summary.children=[caption];summary.querySelector=selector=>selector==='[data-f-selection-label]'?caption:null;
  root.children=[summary,panel];panel.rect={left:0,right:300,top:0,bottom:200,width:300,height:200};
  panel.append=row=>{panel.children=panel.children.filter(child=>child!==row);panel.children.push(row);};
  const options=keys.map(key=>{
    const row=new Node(),choice=new Node(),label=new Node(),text=new Node();row.key=key;text.textContent=key;
    row.dataset={fSelectionKey:key,fCanReorder:String(key!=='fixed'),fOptionDisabled:'false'};
    choice.dataset={fSelectionKey:key,...(key==='fixed'?{fFixed:'true'}:{})};choice.checked=true;
    choice.tagName='INPUT';choice.type='checkbox';choice.parentElement=label;label.parentElement=row;row.parentElement=panel;
    label.children=[choice,text];row.children=[label];choice.closest=selector=>selector==='label'?label:selector==='.f-multi-select-option'?row:null;
    label.closest=selector=>selector==='.f-multi-select-option'?row:null;
    row.closest=selector=>selector==='.f-multi-select-option'?row:row.getAttribute('aria-disabled')==='true'&&selector.includes('aria-disabled')?row:null;
    row.querySelector=selector=>selector==='input[data-f-selection-key]'?choice:selector==='label > span'?text:null;return row;
  });
  panel.children=[...options];
  panel.querySelectorAll=()=>panel.children.flatMap(row=>[row,inputOf(row)]);
  root.querySelector=selector=>selector==='summary'?summary:selector==='.f-multi-select-panel'?panel:null;
  root.querySelectorAll=selector=>selector==='.f-multi-select-option'?panel.children:[];
  root.contains=node=>node===root||node===summary||node===panel||options.some(row=>row===node||row.children.includes(node)||inputOf(row)===node);
  const proxy={invokeMethodAsync(name,...args){snapshots.push({name,args});return Promise.resolve();}};
  const transfer=()=>({values:new Map(),setData(type,value){this.values.set(type,value);}});
  return{root,summary,panel,caption,options,snapshots,proxy,transfer};
}
const inputOf=row=>row.querySelector('input[data-f-selection-key]');
const emptyDataRoot=new Node(),emptyDisplay=selectionFixture('empty-display',[],true);
emptyDisplay.root.parentElement=emptyDataRoot;
emptyDataRoot.querySelector=()=>null;
emptyDataRoot.querySelectorAll=selector=>selector==='[data-f-selection-static="true"]'?[emptyDisplay.root]:[];
check('Empty DataTable attaches the actual standard display once and preserves an open popup on synchronization',()=>{
  dataApi.synchronize(emptyDataRoot,'empty-data');dataApi.synchronize(emptyDataRoot,'empty-data');
  assert.equal(emptyDisplay.root.events.get('toggle').length,1);
  emptyDisplay.root.open=true;emptyDisplay.root.emit('toggle',{});assert.equal(emptyDisplay.panel.popoverOpen,true);
  dataApi.synchronize(emptyDataRoot,'empty-data');assert.equal(emptyDisplay.root.open,true);assert.equal(emptyDisplay.panel.popoverOpen,true);
});
check('Static DataTable disposal releases its actual standard display controller',()=>{
  dataApi.detach('empty-data');assert.equal(emptyDisplay.root.open,false);assert.equal(emptyDisplay.panel.popoverOpen,false);
  assert.equal(emptyDisplay.root.events.get('toggle').length,0);assert.equal(emptyDisplay.summary.events.get('keydown').length,0);
});

function nativeDataFixture(kind='list') {
  const root=new Node(),row=new Node(),target=new Node(),entry=new Node(),submitter=new Node();
  root.children=[row];row.kind=kind;row.entries=[entry];row.nativeOpen=true;
  row.querySelectorAll=selector=>selector==='[data-record-open]'?row.entries:[];
  target.interactive=false;
  target.closest=selector=>selector.startsWith('.f-data-actions')?(target.interactive?target:null)
    :selector==='.f-data-openable[data-f-native-open="true"]'&&row.nativeOpen?row:null;
  entry.tagName='A';entry.setAttribute('href','https://example.test/entry');entry.clicks=0;entry.submits=0;
  entry.closest=selector=>selector==='[inert]'&&entry.inert?entry:null;
  entry.click=()=>entry.clicks++;
  submitter.matches=selector=>selector===':disabled'&&submitter.fieldsetDisabled===true;
  submitter.closest=selector=>selector==='[inert]'&&submitter.inert?submitter:null;
  entry.submitters=[submitter];entry.querySelectorAll=()=>entry.submitters;
  entry.requestSubmit=button=>{assert.equal(button,submitter);entry.submits++;};
  const emit=()=>{const click=event();click.target=target;root.emit('dblclick',click);return click;};
  return {root,row,target,entry,submitter,emit};
}
check('Standard list and card rows synchronously activate the same unique native GET entry once',()=>{
  for(const kind of ['list','cards']) {
    const fixture=nativeDataFixture(kind),id=`native-get-${kind}`;
    dataApi.synchronize(fixture.root,id);dataApi.synchronize(fixture.root,id);
    assert.equal(fixture.root.events.get('dblclick').length,1);
    assert.equal(fixture.root.eventOptions.get(fixture.root.events.get('dblclick')[0]),true);
    const click=fixture.emit();assert.equal(click.propagationStopped,true);assert.equal(fixture.entry.clicks,1);
    // Stopping propagation suppresses the later asynchronous row callback/new-tab duplicate.
    let delayed=0;if(!click.propagationStopped)delayed++;assert.equal(delayed,0);dataApi.detach(id);
  }
});
check('Standard native POST rows use the unique real submitter in list and cards',()=>{
  for(const kind of ['list','cards']) {
    const fixture=nativeDataFixture(kind),id=`native-post-${kind}`;fixture.entry.tagName='FORM';
    dataApi.synchronize(fixture.root,id);assert.equal(fixture.emit().propagationStopped,true);
    assert.equal(fixture.entry.submits,1);assert.equal(fixture.entry.clicks,0);dataApi.detach(id);
  }
});
check('Missing or multiple native opening entries fail closed without a callback fallback',()=>{
  const fixture=nativeDataFixture();dataApi.synchronize(fixture.root,'native-ambiguous');
  fixture.row.entries=[];assert.equal(fixture.emit().propagationStopped,true);
  fixture.row.entries=[fixture.entry,new Node()];assert.equal(fixture.emit().propagationStopped,true);
  fixture.row.entries=[new Node()];assert.equal(fixture.emit().propagationStopped,true);
  assert.equal(fixture.entry.clicks,0);assert.equal(fixture.entry.submits,0);dataApi.detach('native-ambiguous');
});
check('Disabled, busy, inert and href-less native links cannot be activated',()=>{
  const fixture=nativeDataFixture();dataApi.synchronize(fixture.root,'native-unavailable-get');
  fixture.entry.setAttribute('aria-disabled','true');fixture.emit();fixture.entry.removeAttribute('aria-disabled');
  fixture.entry.setAttribute('aria-busy','true');fixture.emit();fixture.entry.removeAttribute('aria-busy');
  fixture.entry.inert=true;fixture.emit();fixture.entry.inert=false;
  fixture.entry.removeAttribute('href');fixture.emit();assert.equal(fixture.entry.clicks,0);dataApi.detach('native-unavailable-get');
});
check('Disabled, fieldset-disabled, busy and inert native POST submitters remain unavailable',()=>{
  const fixture=nativeDataFixture();fixture.entry.tagName='FORM';dataApi.synchronize(fixture.root,'native-unavailable-post');
  fixture.submitter.disabled=true;fixture.emit();fixture.submitter.disabled=false;
  fixture.submitter.fieldsetDisabled=true;fixture.emit();fixture.submitter.fieldsetDisabled=false;
  fixture.submitter.inert=true;fixture.emit();fixture.submitter.inert=false;
  fixture.submitter.setAttribute('aria-disabled','true');fixture.emit();fixture.submitter.removeAttribute('aria-disabled');
  fixture.submitter.setAttribute('aria-busy','true');fixture.emit();assert.equal(fixture.entry.submits,0);dataApi.detach('native-unavailable-post');
});
check('Native POST with no submitter or multiple submitters does not guess a default action',()=>{
  const fixture=nativeDataFixture();fixture.entry.tagName='FORM';dataApi.synchronize(fixture.root,'native-submitters');
  fixture.entry.submitters=[];assert.equal(fixture.emit().propagationStopped,true);
  fixture.entry.submitters=[fixture.submitter,new Node()];assert.equal(fixture.emit().propagationStopped,true);
  assert.equal(fixture.entry.submits,0);dataApi.detach('native-submitters');
});
check('Controls and non-opening or foreign rows do not trigger native double-click activation',()=>{
  const fixture=nativeDataFixture();dataApi.synchronize(fixture.root,'native-source-guard');
  fixture.target.interactive=true;assert.equal(fixture.emit().propagationStopped,false);fixture.target.interactive=false;
  fixture.row.nativeOpen=false;assert.equal(fixture.emit().propagationStopped,false);fixture.row.nativeOpen=true;
  fixture.root.children=[];assert.equal(fixture.emit().propagationStopped,false);
  fixture.root.emit('dblclick',{target:{},stopPropagation(){throw new Error('A non-element source was handled.');}});
  assert.equal(fixture.entry.clicks,0);dataApi.detach('native-source-guard');
});
check('Standard native activation listeners detach completely and reattach without duplication',()=>{
  const fixture=nativeDataFixture();dataApi.synchronize(fixture.root,'native-disposal');dataApi.detach('native-disposal');
  assert.equal(fixture.root.events.get('dblclick').length,0);fixture.emit();assert.equal(fixture.entry.clicks,0);
  dataApi.synchronize(fixture.root,'native-disposal');assert.equal(fixture.root.events.get('dblclick').length,1);
  fixture.emit();assert.equal(fixture.entry.clicks,1);dataApi.detach('native-disposal');
});

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

function dialogViewFixture(){
  const root=new Node(),pending=new Node(),failed=new Node(),paused=new Node(),retry=new Node(),resume=new Node(),footer=new Node(),nestedView=new Node();
  root.dataset={fDialogBrowserControlled:'true',fDialogDismissible:'false'};
  pending.dataset={fDialogView:'pending'};failed.dataset={fDialogView:'failed'};paused.dataset={fDialogView:'paused'};
  retry.dataset={fDialogViews:'failed'};resume.dataset={fDialogViews:'failed paused'};nestedView.dataset={fDialogView:'nested'};
  footer.children=[retry,resume];root.children=[pending,failed,paused,retry,resume,footer];
  for(const child of root.children)child.closest=selector=>selector==='dialog'?root:child.hidden?child:null;
  nestedView.closest=selector=>selector==='dialog'?new Node():null;
  root.querySelectorAll=selector=>selector==='[data-f-dialog-view]'?[pending,failed,paused,nestedView]:selector==='[data-f-dialog-views]'?[retry,resume]:selector==='[autofocus]'?[]:[retry,resume].filter(child=>!child.hidden);
  root.querySelector=selector=>selector==='.f-dialog-actions'?footer:null;
  return {root,pending,failed,paused,retry,resume,footer,nestedView};
}
const nativeViews=dialogViewFixture();
check('Keyed generic dialog views preserve native geometry and declare per-view standard actions',()=>{
  assert.equal(api.setDialogView(nativeViews.root,'pending'),true);assert.equal(nativeViews.pending.hidden,false);assert.equal(nativeViews.failed.hidden,true);
  assert.equal(nativeViews.retry.hidden,true);assert.equal(nativeViews.resume.hidden,true);assert.equal(nativeViews.footer.hidden,true);
  assert.equal(nativeViews.nestedView.hidden,false);assert.equal(nativeViews.root.dataset.fDialogActiveView,'pending');
  assert.equal(api.setDialogView(nativeViews.root,'failed'),true);assert.equal(nativeViews.failed.hidden,false);assert.equal(nativeViews.pending.hidden,true);
  assert.equal(nativeViews.retry.hidden,false);assert.equal(nativeViews.resume.hidden,false);assert.equal(nativeViews.footer.hidden,false);
  assert.equal(api.setDialogView(nativeViews.root,'paused'),true);assert.equal(nativeViews.retry.hidden,true);assert.equal(nativeViews.resume.hidden,false);
});
check('Unknown view keys cannot corrupt the active view or reach a nested dialog',()=>{
  const before=nativeViews.root.dataset.fDialogActiveView;assert.equal(api.setDialogView(nativeViews.root,'unknown'),false);assert.equal(api.setDialogView(nativeViews.root,'nested'),false);
  assert.equal(nativeViews.root.dataset.fDialogActiveView,before);assert.equal(nativeViews.paused.hidden,false);assert.equal(nativeViews.nestedView.hidden,false);
});
const nativeOpener=new Node();document.activeElement=nativeOpener;
api.setDialogView(nativeViews.root,'failed');api.synchronizeDialog(nativeViews.root,true);
check('Browser dialog opens without a circuit reference and moves focus away from a newly hidden action',()=>{
  assert.equal(nativeViews.root.open,true);assert.equal(document.activeElement,nativeViews.retry);
  api.setDialogView(nativeViews.root,'paused');assert.equal(document.activeElement,nativeViews.resume);assert.equal(nativeViews.root.open,true);
});
check('Non-dismissable browser dialog prevents Escape and its close action',()=>{
  const cancel=event();nativeViews.root.emit('cancel',cancel);assert.equal(cancel.defaultPrevented,true);assert.equal(nativeViews.root.open,true);
  const target=new Node();target.closest=()=>target;nativeViews.root.emit('click',{target,preventDefault(){}});assert.equal(nativeViews.root.open,true);
});
check('Busy browser dialog retains its native modal until an enabled dismissal restores the opener',()=>{
  nativeViews.root.dataset.fDialogDismissible='true';nativeViews.root.setAttribute('aria-busy','true');nativeViews.root.emit('cancel',event());assert.equal(nativeViews.root.open,true);
  nativeViews.root.setAttribute('aria-busy','false');nativeViews.root.emit('cancel',event());assert.equal(nativeViews.root.open,false);assert.equal(document.activeElement,nativeOpener);
});
check('Browser dialog disposal releases native close cancel and focus listeners without circuit callbacks',()=>{
  api.synchronizeDialog(nativeViews.root,true);api.detachDialog(nativeViews.root);assert.equal(nativeViews.root.open,false);
  for(const type of ['click','cancel','keydown'])assert.equal(nativeViews.root.events.get(type).length,0);
  nativeViews.root.emit('cancel',event());assert.equal(nativeViews.root.open,false);
});

check('Keyed actions inside InlineActions hide and restore the same native dialog footer',()=>{
  const fixture=dialogViewFixture(),group=new Node();group.children=[fixture.retry,fixture.resume];fixture.footer.children=[group];
  group.closest=selector=>selector==='dialog'?fixture.root:group.hidden?group:null;
  fixture.footer.querySelectorAll=()=>group.children;
  api.setDialogView(fixture.root,'pending');assert.equal(group.hidden,false);assert.equal(fixture.footer.hidden,true);
  api.setDialogView(fixture.root,'failed');assert.equal(fixture.footer.hidden,false);assert.equal(fixture.retry.hidden,false);assert.equal(fixture.resume.hidden,false);
  api.setDialogView(fixture.root,'pending');assert.equal(fixture.footer.hidden,true);
  api.setDialogView(fixture.root,'paused');assert.equal(fixture.footer.hidden,false);assert.equal(fixture.retry.hidden,true);assert.equal(fixture.resume.hidden,false);
});
check('Native browser close actions only dismiss the nearest owned dialog including nested icon sources',()=>{
  const outer=new Node(),inner=new Node(),outerClose=new Node(),innerClose=new Node(),innerIcon=new Node();
  outer.dataset={fDialogBrowserControlled:'true',fDialogDismissible:'true'};inner.dataset={...outer.dataset};outer.children=[outerClose];inner.children=[innerClose];
  outerClose.closest=selector=>selector==='dialog'?outer:null;innerClose.closest=selector=>selector==='dialog'?inner:null;
  innerIcon.closest=selector=>selector==='[data-f-dialog-close]'?innerClose:inner;
  api.synchronizeDialog(outer,true);api.synchronizeDialog(inner,true);
  let prevented=0;const click={target:innerIcon,preventDefault(){prevented++;}};
  outer.emit('click',click);assert.equal(outer.open,true);assert.equal(inner.open,true);assert.equal(prevented,0);
  inner.emit('click',click);assert.equal(inner.open,false);assert.equal(outer.open,true);assert.equal(prevented,1);
  const outerIcon=new Node();outerIcon.closest=selector=>selector==='[data-f-dialog-close]'?outerClose:outer;
  outer.emit('click',{target:outerIcon,preventDefault(){prevented++;}});assert.equal(outer.open,false);assert.equal(prevented,2);
  api.detachDialog(inner);api.detachDialog(outer);
});

check('Validation skips disabled or invisible errors',()=>{const root=new Node(),disabled=new Node(),hidden=new Node(),invalid=new Node();disabled.disabled=true;hidden.hidden=true;root.children=[disabled,hidden,invalid];assert.equal(api.focusFirstInvalid(root),true);assert.equal(document.activeElement,invalid);assert.equal(invalid.scrolled,true);});
const unsupportedMenuTrigger=new Node(),unsupportedMenu=new Node();unsupportedMenu.showPopover=undefined;
check('Menu requires the native Popover API and never creates a compatibility portal',()=>{assert.throws(()=>api.toggleMenu(unsupportedMenuTrigger,unsupportedMenu),/Native Popover API/);assert.equal(document.body.children.includes(unsupportedMenu),false);api.detachMenu(unsupportedMenuTrigger,unsupportedMenu);});
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
const unsupportedDialog=new Node();unsupportedDialog.showModal=undefined;
check('Dialog requires the native modal API without constructing a compatibility backdrop',()=>{assert.throws(()=>api.synchronizeDialog(unsupportedDialog,true,reference),/Native Dialog API/);assert.equal(Boolean(unsupportedDialog.open),false);assert.equal(document.body.children.includes(unsupportedDialog),false);});
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
function columnReorderFixture(progressive=true){
  const f=progressiveFixture(),group=new Node(),spacer=new Node();
  if(!progressive)f.root.dataset.fProgressive='false';
  const keys=['name','fixed','code','detail'],display=selectionFixture('display-'+Math.random(),keys,progressive);
  display.root.parentElement=f.root;display.root.dataset.fTableSelection='true';
  const rootQuery=f.root.querySelector;
  f.root.querySelector=selector=>selector==='[data-f-table-selection="true"]'?display.root:rootQuery(selector);
  group.children=[...keys.map(key=>{const col=new Node();col.dataset={fColumn:key};return col;}),spacer];
  group.querySelector=()=>spacer;group.insertBefore=(cell,before)=>{group.children=group.children.filter(child=>child!==cell);group.children.splice(group.children.indexOf(before),0,cell);};
  const rootAll=f.root.querySelectorAll;
  f.root.querySelectorAll=selector=>selector==='[data-f-selection-static="true"]'?(progressive?[display.root]:[]):rootAll(selector);
  const table=f.root.querySelector('[data-f-table]'),tableQuery=table.querySelector;
  table.querySelector=selector=>selector==='colgroup'?group:tableQuery(selector);
  table.querySelectorAll=selector=>selector==='[data-f-column]'?group.children.filter(col=>col.dataset.fColumn):[];
  return{...f,parent:display.panel,display:display.root,group,keys,options:display.options,transfer:display.transfer,controls:display};
}
check('Progressive display items drag through the unique controller with a real payload and fixed slots',()=>{
  const f=columnReorderFixture(),{parent,group,keys,options}=f;dataApi.synchronize(f.root,'progressive-order');
  const drag=(source,target)=>{
    const dataTransfer=f.transfer(),start={...event(''),target:source,dataTransfer};
    f.display.emit('dragstart',start);
    if(!start.defaultPrevented){assert.equal(dataTransfer.values.get('text/plain'),source.key);assert.equal(dataTransfer.effectAllowed,'move');}
    f.display.emit('dragover',{...event(''),target,dataTransfer});
    f.display.emit('drop',{...event(''),target});f.display.emit('dragend',event(''));
  };
  drag(options[0],options[3]);
  assert.deepEqual(parent.children.map(option=>option.key),['code','fixed','detail','name']);
  assert.deepEqual(group.children.filter(col=>col.dataset.fColumn).map(col=>col.dataset.fColumn),['code','fixed','detail','name']);
  drag(options[0],options[2]);assert.deepEqual(parent.children.map(option=>option.key),keys);
  drag(options[0],options[1]);drag(options[1],options[3]);assert.deepEqual(parent.children.map(option=>option.key),keys);
  assert.ok(options.every(option=>option.getAttribute('data-f-dragging')===null));dataApi.detach('progressive-order');
});
check('Progressive table ignores a generic nested selector event without altering column state',()=>{
  const f=columnReorderFixture();dataApi.synchronize(f.root,'progressive-isolation');
  const unrelated=selectionFixture('unrelated-selection',['name','fixed','code','detail'],true);
  f.root.emit('flourish-selection-change',{...event(''),target:unrelated.root,detail:{orderedKeys:['detail','code','fixed','name'],selectedKeys:['detail']}});
  assert.deepEqual(f.group.children.filter(col=>col.dataset.fColumn).map(col=>col.dataset.fColumn),f.keys);
  assert.ok(f.group.children.filter(col=>col.dataset.fColumn).every(col=>!col.hidden));
  dataApi.detach('progressive-isolation');
});
check('Progressive standard display keyboard moves whole rows and Cards only freezes ordering',()=>{
  const f=columnReorderFixture();dataApi.synchronize(f.root,'progressive-row-keys');
  const key={...event('ArrowUp'),target:f.options[2]};f.display.emit('keydown',key);
  assert.equal(key.defaultPrevented,true);assert.deepEqual(f.parent.children.map(option=>option.key),['code','fixed','name','detail']);
  const fixed={...event('ArrowDown'),target:f.options[1]};f.display.emit('keydown',fixed);assert.equal(fixed.defaultPrevented,false);
  f.view.dataset.fView='cards';f.root.emit('click',{...event(''),target:f.view});
  assert.ok(f.options.every(option=>option.getAttribute('draggable')==='false'));
  assert.equal(inputOf(f.options[2]).disabled,false,'Cards must not disable visibility selection');
  const locked={...event(''),target:f.options[0],dataTransfer:f.transfer()};f.display.emit('dragstart',locked);assert.equal(locked.defaultPrevented,true);
  f.view.dataset.fView='table';f.root.emit('click',{...event(''),target:f.view});
  assert.equal(f.options[1].getAttribute('draggable'),'false');assert.ok(f.options.filter(option=>option.key!=='fixed').every(option=>option.getAttribute('draggable')==='true'));
  dataApi.detach('progressive-row-keys');assert.ok([...f.display.events.values()].every(list=>list.length===0));
});
check('Interactive standard display publishes the same validated full snapshot rather than invoking a table-specific controller',()=>{
  const f=columnReorderFixture(false),child=new Node();child.closest=()=>f.options[0];
  selectionApi.synchronize(f.display,f.display.dataset.fSelectionInstance,f.controls.proxy);
  const dataTransfer=f.transfer();f.display.emit('dragstart',{...event(''),target:child,dataTransfer});
  assert.equal(dataTransfer.values.get('text/plain'),'name');assert.equal(f.options[0].getAttribute('data-f-dragging'),'');
  f.display.emit('drop',{...event(''),target:f.options[3]});
  assert.deepEqual(f.controls.snapshots.at(-1),{name:'ApplyAsync',args:[['code','fixed','detail','name'],['code','fixed','detail','name']]});
  const fixed={...event(''),target:f.options[1],dataTransfer:f.transfer()};f.display.emit('dragstart',fixed);assert.equal(fixed.defaultPrevented,true);
  const absent={...event(''),target:f.options[0]};f.display.emit('dragstart',absent);assert.equal(absent.defaultPrevented,true);
  selectionApi.detach(f.display.dataset.fSelectionInstance);assert.ok([...f.display.events.values()].every(list=>list.length===0));
});
check('Display row reorder keys stop at the unique controller instead of also navigating the popup',()=>{
  const f=selectionFixture('keyboard-popup',['one','two']);
  selectionApi.synchronize(f.root,'keyboard-popup',f.proxy);f.root.open=true;f.root.emit('toggle',{});f.options[0].focus();
  const down={...event('ArrowDown'),target:f.options[0]};f.root.emit('keydown',down);document.emit('keydown',down);
  assert.equal(down.defaultPrevented,true);assert.equal(down.propagationStopped,true);assert.equal(document.activeElement,f.options[0]);
  selectionApi.detach('keyboard-popup');assert.equal(f.root.open,false);
});
check('Standard display dragging cannot cross component instances or use a cancelled source',()=>{
  const first=selectionFixture('first-display',['one','two']),second=selectionFixture('second-display',['one','two']);
  selectionApi.synchronize(first.root,'first-display',first.proxy);selectionApi.synchronize(second.root,'second-display',second.proxy);
  first.root.emit('dragstart',{...event(''),target:first.options[0],dataTransfer:first.transfer()});
  second.root.emit('drop',{...event(''),target:second.options[1]});assert.equal(second.snapshots.length,0);
  first.root.emit('keydown',{...event('Escape'),target:first.options[0]});first.root.emit('drop',{...event(''),target:first.options[1]});assert.equal(first.snapshots.length,0);
  first.root.emit('dragstart',{...event(''),target:second.options[0],dataTransfer:first.transfer()});first.root.emit('drop',{...event(''),target:first.options[1]});assert.equal(first.snapshots.length,0);
  selectionApi.detach('first-display');selectionApi.detach('second-display');
});
check('Standard display visibility rejects fixed minimum and disabled choices through real change listeners',()=>{
  const f=selectionFixture('visibility',['fixed','one','two']);f.root.dataset.fMinimumSelected='1';
  selectionApi.synchronize(f.root,'visibility',f.proxy);
  inputOf(f.options[0]).checked=false;f.root.emit('change',{...event(''),target:inputOf(f.options[0])});assert.equal(inputOf(f.options[0]).checked,true);assert.equal(f.snapshots.length,0);
  inputOf(f.options[1]).checked=false;f.root.emit('change',{...event(''),target:inputOf(f.options[1])});assert.equal(f.snapshots.length,1);
  inputOf(f.options[2]).checked=false;f.root.emit('change',{...event(''),target:inputOf(f.options[2])});assert.equal(f.snapshots.length,2);
  assert.equal(inputOf(f.options[0]).disabled,true);
  f.root.dataset.fSelectionDisabled='true';selectionApi.synchronize(f.root,'visibility',f.proxy);
  inputOf(f.options[1]).checked=true;f.root.emit('change',{...event(''),target:inputOf(f.options[1])});assert.equal(inputOf(f.options[1]).checked,false);assert.equal(f.snapshots.length,2);
  selectionApi.detach('visibility');
});
check('The same display controller preserves native top-layer outside-close and Escape focus restoration',()=>{
  const f=selectionFixture('display-close',['one']);selectionApi.synchronize(f.root,'display-close',f.proxy);
  f.root.open=true;f.root.emit('toggle',{});assert.equal(f.panel.popoverOpen,true);
  const outside=new Node();outside.focus();document.emit('pointerdown',{target:outside});assert.equal(f.root.open,false);assert.equal(document.activeElement,outside);
  f.root.open=true;f.root.emit('toggle',{});document.emit('keydown',event('Escape'));assert.equal(f.root.open,false);assert.equal(document.activeElement,f.summary);
  selectionApi.detach('display-close');
});
check('Minimum visible protects the final unfixed checkbox and remains recoverable',()=>{
  const f=selectionFixture('last-visible',['one','two']);f.root.dataset.fMinimumSelected='1';
  inputOf(f.options[1]).checked=false;selectionApi.synchronize(f.root,'last-visible',f.proxy);
  assert.equal(inputOf(f.options[0]).disabled,true);
  inputOf(f.options[0]).checked=false;f.root.emit('change',{...event(''),target:inputOf(f.options[0])});
  assert.equal(inputOf(f.options[0]).checked,true);assert.equal(f.snapshots.length,0);
  inputOf(f.options[1]).checked=true;f.root.emit('change',{...event(''),target:inputOf(f.options[1])});
  assert.equal(inputOf(f.options[0]).disabled,false);assert.equal(f.snapshots.length,1);selectionApi.detach('last-visible');
});
check('Removed or now-disabled rows cannot complete an earlier standard display drag',()=>{
  const f=selectionFixture('stale-drag',['one','two']);selectionApi.synchronize(f.root,'stale-drag',f.proxy);
  f.root.emit('dragstart',{...event(''),target:f.options[0],dataTransfer:f.transfer()});
  f.options[0].isConnected=false;selectionApi.synchronize(f.root,'stale-drag',f.proxy);
  f.root.emit('drop',{...event(''),target:f.options[1]});assert.equal(f.snapshots.length,0);
  f.options[0].isConnected=true;f.root.emit('dragstart',{...event(''),target:f.options[0],dataTransfer:f.transfer()});
  f.options[0].dataset.fOptionDisabled='true';selectionApi.synchronize(f.root,'stale-drag',f.proxy);
  f.root.emit('drop',{...event(''),target:f.options[1]});assert.equal(f.snapshots.length,0);selectionApi.detach('stale-drag');
});
check('MultiSelectBox maximum selections blocks unchecked choices and remains recoverable',()=>{
  const f=selectionFixture('selection-maximum',['one','two']);f.root.dataset.fMaximumSelections='1';
  inputOf(f.options[1]).checked=false;selectionApi.synchronize(f.root,'selection-maximum',f.proxy);
  assert.equal(inputOf(f.options[1]).disabled,true);assert.equal(inputOf(f.options[0]).disabled,false);
  inputOf(f.options[1]).checked=true;f.root.emit('change',{...event(''),target:inputOf(f.options[1])});
  assert.equal(inputOf(f.options[1]).checked,false);assert.equal(f.snapshots.length,0);
  inputOf(f.options[0]).checked=false;f.root.emit('change',{...event(''),target:inputOf(f.options[0])});
  assert.equal(inputOf(f.options[1]).disabled,false);assert.equal(f.snapshots.length,1);selectionApi.detach('selection-maximum');
});
check('MultiSelectBox filtering prevents partial reorder while preserving full selection snapshots',()=>{
  const f=selectionFixture('selection-filter',['one','two']);f.root.dataset.fFiltering='true';f.options[1].hidden=true;
  selectionApi.synchronize(f.root,'selection-filter',f.proxy);
  assert.ok(f.options.every(row=>row.getAttribute('draggable')==='false'));
  const drag={...event(''),target:f.options[0],dataTransfer:f.transfer()};f.root.emit('dragstart',drag);assert.equal(drag.defaultPrevented,true);
  inputOf(f.options[0]).checked=false;f.root.emit('change',{...event(''),target:inputOf(f.options[0])});
  assert.deepEqual(f.snapshots.at(-1),{name:'ApplyAsync',args:[['one','two'],['two']]});
  f.root.dataset.fFiltering='false';f.options[1].hidden=false;selectionApi.synchronize(f.root,'selection-filter',f.proxy);
  assert.ok(f.options.every(row=>row.getAttribute('draggable')==='true'));selectionApi.detach('selection-filter');
});
check('MultiSelectBox pending creation freezes delayed selection and drag then recovers',()=>{
  const f=selectionFixture('selection-create',['one','two']);f.root.dataset.fCreating='true';
  selectionApi.synchronize(f.root,'selection-create',f.proxy);
  assert.ok(f.options.every(row=>inputOf(row).disabled&&row.getAttribute('draggable')==='false'));
  inputOf(f.options[0]).checked=false;f.root.emit('change',{...event(''),target:inputOf(f.options[0])});
  assert.equal(inputOf(f.options[0]).checked,true);assert.equal(f.snapshots.length,0);
  f.root.dataset.fCreating='false';selectionApi.synchronize(f.root,'selection-create',f.proxy);
  inputOf(f.options[0]).checked=false;f.root.emit('change',{...event(''),target:inputOf(f.options[0])});
  assert.equal(f.snapshots.length,1);selectionApi.detach('selection-create');
});
check('Standalone native MultiSelectBox emits selected keys once and detach removes its listeners',()=>{
  const f=selectionFixture('selection-static',['one','two'],true),events=[];
  f.root.addEventListener('flourish-selection-change',evt=>events.push(evt.detail));
  selectionApi.synchronize(f.root,'selection-static');selectionApi.synchronize(f.root,'selection-static');
  inputOf(f.options[1]).checked=false;f.root.emit('change',{...event(''),target:inputOf(f.options[1])});
  assert.deepEqual(events,[{orderedKeys:['one','two'],selectedKeys:['one']}]);
  selectionApi.detach('selection-static');assert.equal(f.root.events.get('change').length,0);
  assert.equal(f.root.events.get('dragstart').length,0);assert.equal(f.summary.events.get('keydown').length,0);
});
check('Native selection caption follows all memberships and preserves an explicit host label',()=>{
  const f=selectionFixture('selection-caption',['First <account>','Second account'],true),events=[];
  f.root.dataset.fSelectionEmpty='No account selected';f.root.dataset.fSelectionCount='accounts selected';
  f.root.addEventListener('flourish-selection-change',evt=>events.push(evt.detail));
  selectionApi.synchronize(f.root,'selection-caption');assert.equal(f.caption.textContent,'2 accounts selected');
  f.options[1].hidden=true;f.root.dataset.fFiltering='true';selectionApi.synchronize(f.root,'selection-caption');
  assert.equal(f.caption.textContent,'2 accounts selected','Filtered members must still contribute to the caption');
  inputOf(f.options[1]).checked=false;f.root.emit('change',{...event(''),target:inputOf(f.options[1])});
  assert.equal(f.caption.textContent,'First <account>');
  assert.deepEqual(events.at(-1),{orderedKeys:['First <account>','Second account'],selectedKeys:['First <account>']});
  inputOf(f.options[0]).checked=false;f.root.emit('change',{...event(''),target:inputOf(f.options[0])});assert.equal(f.caption.textContent,'No account selected');
  f.root.dataset.fSelectionCaption='Accounts';f.caption.textContent='Accounts';
  inputOf(f.options[1]).checked=true;f.root.emit('change',{...event(''),target:inputOf(f.options[1])});
  assert.equal(f.caption.textContent,'Accounts','A caller label must retain its identity while selection changes');
  selectionApi.detach('selection-caption');
});
function measuredTableFixture() {
  const root=new Node(),table=new Node(),action=new Node();
  const cols=['name','code'].map(key=>{const col=new Node();col.dataset={fColumn:key};return col;});
  cols[1].dataset.fLastColumn='true';action.rect.width=80;
  const handles=cols.map(col=>{const button=new Node();button.dataset={fResize:col.dataset.fColumn};button.rect.width=8;return button;});
  const texts=cols.map(()=>{const text=new Node();text.textContent='x'.repeat(80);return text;});
  const cells=cols.map((col,index)=>{const cell=new Node();cell.dataset={fColumn:col.dataset.fColumn};cell.querySelector=selector=>selector==='[data-f-resize]'?handles[index]:texts[index];return cell;});
  table.querySelectorAll=selector=>selector==='col[data-f-column]'?cols:selector==='th[data-f-column],td[data-f-column]'?cells:selector==='[data-f-resize]'?handles:[];
  table.querySelector=selector=>selector==='th.f-data-actions'?action:null;
  root.querySelector=selector=>selector==='[data-f-table]'?table:null;root.dataset={};
  return {root,table,cols,handles,texts};
}
check('Canonical widths measure content with a nonfinal cap and uncapped final column',()=>{
  const f=measuredTableFixture();dataApi.synchronize(f.root,'measurement');
  assert.equal(f.cols[0].style.width,'320px');assert.equal(f.cols[1].style.width,'728px');
  assert.equal(f.handles[0].getAttribute('aria-valuenow'),'320');assert.equal(f.handles[1].getAttribute('aria-valuenow'),'728');
  assert.equal(f.table.style.minWidth,'1128px');dataApi.detach('measurement');
});
check('Manual widths survive measurements, reject invalid values, reset naturally and are instance-local',()=>{
  const first=measuredTableFixture(),second=measuredTableFixture();
  dataApi.synchronize(first.root,'first-width');dataApi.setColumnWidth(first.root,'first-width','name',450);
  first.texts[0].textContent='short';dataApi.synchronize(first.root,'first-width');
  assert.equal(first.cols[0].style.width,'450px');assert.equal(first.handles[0].getAttribute('aria-valuenow'),'450');
  dataApi.setColumnWidth(first.root,'first-width','name',NaN);dataApi.setColumnWidth(first.root,'first-width','unknown',200);
  assert.equal(first.cols[0].style.width,'450px');
  dataApi.synchronize(second.root,'second-width');assert.equal(second.cols[0].style.width,'320px');
  dataApi.setColumnWidth(first.root,'first-width','name',null);assert.equal(first.cols[0].style.width,'72px');
  assert.equal(first.handles[0].getAttribute('aria-valuenow'),'72');dataApi.detach('first-width');dataApi.detach('second-width');
});
check('Declared hidden column widths are cached without accepting absent unknown keys',()=>{
  const f=measuredTableFixture();
  const choices=f.cols.map(col=>({dataset:{fSelectionKey:col.dataset.fColumn}}));
  f.root.querySelectorAll=selector=>selector==='input[data-f-selection-key]'?choices:[];
  dataApi.synchronize(f.root,'hidden-width');f.cols[0].hidden=true;
  dataApi.setColumnWidth(f.root,'hidden-width','name',487);dataApi.setColumnWidth(f.root,'hidden-width','unknown',777);
  f.cols[0].hidden=false;
  const unknown=new Node();unknown.dataset={fColumn:'unknown'};f.cols.push(unknown);choices.push({dataset:{fSelectionKey:'unknown'}});
  dataApi.synchronize(f.root,'hidden-width');
  assert.equal(f.cols[0].style.width,'487px');assert.equal(f.handles[0].getAttribute('aria-valuenow'),'487');
  assert.equal(unknown.style.width,'72px');dataApi.detach('hidden-width');
});
check('Cards can set and reset declared widths before returning to the table without accepting unknown keys',()=>{
  const f=measuredTableFixture(),choices=f.cols.map(col=>({dataset:{fSelectionKey:col.dataset.fColumn}}));
  f.root.querySelectorAll=selector=>selector==='input[data-f-selection-key]'?choices:[];
  dataApi.synchronize(f.root,'cards-width');
  const originalQuery=f.root.querySelector;f.root.querySelector=()=>null;
  dataApi.setColumnWidth(f.root,'cards-width','name',451);dataApi.setColumnWidth(f.root,'cards-width','code',511);
  dataApi.setColumnWidth(f.root,'cards-width','code',Infinity);dataApi.setColumnWidth(f.root,'cards-width','unknown',900);
  dataApi.setColumnWidth(f.root,'cards-width','name',null);
  f.root.querySelector=originalQuery;const unknown=new Node();unknown.dataset={fColumn:'unknown'};
  f.cols.push(unknown);choices.push({dataset:{fSelectionKey:'unknown'}});dataApi.synchronize(f.root,'cards-width');
  assert.equal(f.cols[0].style.width,'320px');assert.equal(f.cols[1].style.width,'511px');
  assert.equal(f.handles[1].getAttribute('aria-valuenow'),'511');assert.equal(unknown.style.width,'72px');dataApi.detach('cards-width');
});
process.stdout.write(`${count}/${count} mocked DOM checks passed.\n`);
