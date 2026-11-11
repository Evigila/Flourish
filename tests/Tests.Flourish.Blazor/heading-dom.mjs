import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
const assets = new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/',import.meta.url);
const source = await readFile(new URL('shell.js',assets),'utf8');
const { attach, detach, updateCompactHeading, resetCompactHeading } = await import('data:text/javascript;base64,'+Buffer.from(source.replace(/^import \{ initializePrimaryNavigationPreviews.*$/m,'const initializePrimaryNavigationPreviews=()=>{}; const disposePrimaryNavigationPreviews=()=>{};')).toString('base64'));
const controller = await readFile(new URL('patterns/page-heading.js',assets),'utf8');
const { initializePageHeadings } = await import('data:text/javascript;base64,'+Buffer.from(controller.replace(/^import.*refreshAnchoredPopups.*$/m,'const refreshAnchoredPopups=root=>{root.popupRefreshes=(root.popupRefreshes??0)+1;};')).toString('base64'));

function fixture() {
    const headings=[],observers=[],frames=new Map(),events=new Map();
    let serial=0,writes=0;
    const eventTarget=()=>({addEventListener(name,fn){events.set(this.name+name,fn);},removeEventListener(name){events.delete(this.name+name);}});
    class Observer {
        constructor(callback){this.callback=callback;this.targets=new Set();observers.push(this);}
        observe(node){this.targets.add(node);}
        unobserve(node){this.targets.delete(node);}
        disconnect(){this.targets.clear();this.closed=true;}
    }
    const win={...eventTarget(),name:'window',innerHeight:800,
        ResizeObserver:Observer,MutationObserver:Observer,
        requestAnimationFrame(fn){frames.set(++serial,fn);return serial;},
        cancelAnimationFrame(id){frames.delete(id);},
        getComputedStyle(node){return {overflowX:'visible',overflowY:'visible',visibility:'visible',paddingLeft:'24px',paddingRight:'24px',...node.computed};}};
    const doc={...eventTarget(),name:'document',defaultView:win,documentElement:{clientWidth:1200},
        querySelector:()=>headings[0]?.overlay,
        querySelectorAll:()=>headings};
    function node(parent=null) {
        const attrs=new Map(),styles=new Map();
        return {ownerDocument:doc,parentElement:parent,isConnected:true,computed:{},
            style:{getPropertyValue:key=>styles.get(key)??'',setProperty(key,value){styles.set(key,value);writes++;},removeProperty(key){if(styles.delete(key))writes++;}},
            contains(other){for(let item=other;item;item=item.parentElement)if(item===this)return true;return false;},
            getAttribute:key=>attrs.get(key)??null,hasAttribute:key=>attrs.has(key),
            setAttribute(key,value=''){if(attrs.get(key)!==value){attrs.set(key,value);writes++;}},
            removeAttribute(key){if(attrs.delete(key))writes++;},
            getClientRects(){return this.hidden?[]:[this.getBoundingClientRect()];},
            closest(selector){
                for(let item=this;item;item=item.parentElement){
                    if(selector==='.f-page-body'&&item.page)return item;
                    if(selector==='.f-document-surface'&&item.documentFlow)return item;
                    if(selector.startsWith('[hidden]')&&item.hidden)return item;
                }return null;
            }};
    }
    function region(left=100,top=100,width=900,height=600,parent=doc.documentElement) {
        const stage=node(parent);Object.assign(stage,{clientLeft:0,clientTop:0,clientWidth:width,clientHeight:height,scrollTop:0,scrollHeight:1900,left,top});
        stage.computed={overflowX:'auto',overflowY:'auto'};
        stage.getBoundingClientRect=()=>({left:stage.left,right:stage.left+stage.clientWidth,top:stage.top,bottom:stage.top+stage.clientHeight});
        return stage;
    }
    function add(stage=region()) {
        const page=node(stage);page.page=true;page.getBoundingClientRect=()=>({bottom:stage.top+1900-stage.scrollTop});
        const heading=node(page),overlay=node(heading);
        heading.height=174;heading.title='Page title';heading.overlay=overlay;
        heading.getBoundingClientRect=()=>({left:stage.left,right:stage.left+stage.clientWidth,width:stage.clientWidth,top:stage.top-stage.scrollTop,bottom:stage.top+heading.height-stage.scrollTop});
        overlay.origin={left:0,top:0};overlay.getBoundingClientRect=()=>overlay.origin;
        heading.querySelectorAll=()=>[];
        overlay.querySelector=()=>null;
        heading.querySelector=selector=>selector==='[data-f-heading-overlay]'?overlay:selector==='h1'?{textContent:heading.title}:null;
        headings.push(heading);return {stage,page,heading,overlay};
    }
    function controls(x) {
        const values=[];
        for(const [kind,width] of [['parent',130],['actions',240]]) {
            const slot=node(x.heading),anchor=node(slot),content=node(anchor),space=node(x.overlay);
            slot.setAttribute('data-f-heading-control-slot',kind);
            anchor.firstElementChild=content;slot.querySelector=()=>anchor;
            content.naturalWidth=width;content.naturalHeight=48;content.id=kind+'-unique';
            anchor.getBoundingClientRect=()=>({left:parseFloat(anchor.style.getPropertyValue('--f-heading-control-left'))||0,
                top:parseFloat(anchor.style.getPropertyValue('--f-heading-control-top'))||0,
                width:parseFloat(anchor.style.getPropertyValue('--f-heading-control-width'))||width});
            content.getBoundingClientRect=()=> {
                const fixed=anchor.hasAttribute('data-f-heading-fixed'),width=fixed?anchor.getBoundingClientRect().width:content.naturalWidth;
                return {width,height:content.hidden?0:fixed&&kind==='actions'?Math.ceil(content.naturalWidth/Math.max(1,width))*content.naturalHeight:content.naturalHeight};
            };
            slot.getBoundingClientRect=()=>{
                const reserved=slot.hasAttribute('data-f-heading-reserved');
                const width=reserved?parseFloat(slot.style.getPropertyValue('--f-heading-slot-width')):content.hidden?0:content.naturalWidth;
                const height=reserved?parseFloat(slot.style.getPropertyValue('--f-heading-slot-height')):content.hidden?0:content.naturalHeight+(kind==='parent'?8:0);
                const top=x.stage.top-x.stage.scrollTop+(kind==='parent'?42:100);
                return {left:x.stage.left+24,top,bottom:top+height,width,height};
            };
            Object.defineProperties(space,{
                offsetLeft:{get:()=>kind==='parent'?24:x.overlay.hasAttribute('data-f-heading-stacked')?24:x.stage.clientWidth-24-parseFloat(x.overlay.style.getPropertyValue('--f-heading-actions-width'))},
                offsetTop:{get:()=>kind==='actions'&&x.overlay.hasAttribute('data-f-heading-stacked')?72:12}
            });
            values.push({kind,slot,anchor,content,space});
        }
        x.heading.querySelectorAll=()=>values.map(item=>item.slot);
        x.overlay.querySelector=selector=>values.find(item=>selector==='[data-f-heading-'+item.kind+'-space]')?.space??null;
        return values;
    }
    function flush(){
        let count=0;
        while(frames.size){const tasks=[...frames.values()];frames.clear();tasks.forEach(fn=>fn());assert.ok(++count<5,'observer did not settle');}
    }
    const changed=()=>{observers.filter(x=>!x.closed).forEach(x=>x.callback());flush();};
    return {doc,win,headings,observers,events,frames,add,region,controls,flush,changed,writes:()=>writes,
        scroll(){events.get('documentscroll')?.();flush();}};
}
const visible=overlay=>overlay.hasAttribute('data-f-heading-visible');
const px=(overlay,name)=>parseFloat(overlay.style.getPropertyValue('--f-heading-overlay-'+name));

test('the whole natural heading must leave the visible region; return reverses at the same geometric boundary',()=>{
    const f=fixture(),x=f.add();initializePageHeadings(f.doc);
    x.stage.scrollTop=173;f.scroll();assert.equal(visible(x.overlay),false);
    x.stage.scrollTop=174;f.scroll();assert.equal(visible(x.overlay),true);
    assert.equal(x.stage.scrollHeight,1900);assert.equal(x.heading.height,174);assert.equal(x.stage.scrollTop,174);
    x.stage.scrollTop=173;f.scroll();assert.equal(visible(x.overlay),false);
    const writes=f.writes();f.changed();assert.equal(f.writes(),writes);
});
test('short documents never collapse their original title or change available scroll height',()=>{
    const f=fixture(),x=f.add();x.stage.scrollHeight=650;x.stage.scrollTop=50;
    initializePageHeadings(f.doc);assert.equal(visible(x.overlay),false);
    assert.equal(x.stage.scrollTop,50);assert.equal(x.stage.scrollHeight,650);
    const attrs=new Set(['data-compact-heading']);
    const root={removeAttribute:key=>attrs.delete(key)};
    updateCompactHeading(root,x.stage);resetCompactHeading(root);
    assert.equal(attrs.size,0);assert.equal(x.stage.scrollTop,50);
});
test('nested scroll clipping and translated fixed containing blocks bound the independent overlay',()=>{
    const f=fixture(),outer=f.region(120,150,600,350),inner=f.region(100,100,900,600,outer),x=f.add(inner);
    x.stage.scrollTop=125;x.overlay.origin={left:20,top:30};
    initializePageHeadings(f.doc);
    assert.equal(visible(x.overlay),true);assert.equal(px(x.overlay,'top'),120);
    assert.equal(px(x.overlay,'left'),100);assert.equal(px(x.overlay,'width'),600);assert.equal(px(x.overlay,'limit'),350);
    outer.top=180;outer.clientWidth=400;f.changed();
    assert.equal(px(x.overlay,'top'),150);assert.equal(px(x.overlay,'width'),400);
});
test('viewport and heading resize update the boundary without cumulative measurements',()=>{
    const f=fixture(),x=f.add();x.stage.scrollTop=180;
    f.win.visualViewport={name:'viewport',offsetLeft:130,offsetTop:160,width:400,height:300,
        addEventListener(name,fn){f.events.set('viewport'+name,fn);},removeEventListener(name){f.events.delete('viewport'+name);}};
    initializePageHeadings(f.doc);assert.equal(visible(x.overlay),true);
    assert.equal(px(x.overlay,'top'),160);assert.equal(px(x.overlay,'width'),400);
    x.heading.height=300;f.changed();assert.equal(visible(x.overlay),false);
    f.win.visualViewport.height=200;f.events.get('viewportresize')();f.flush();assert.equal(px(x.overlay,'limit'),200);
    assert.ok(f.observers[0].targets.has(x.heading));assert.ok(f.observers[0].targets.has(x.stage));
});
test('explicit Compact, DocumentFlow, hidden and empty headings do not gain automatic titles',()=>{
    for(const mode of ['compact','document','hidden','empty']){
        const f=fixture(),x=f.add();x.stage.scrollTop=500;
        if(mode==='compact')x.heading.setAttribute('data-heading-mode','compact');
        if(mode==='document')x.stage.documentFlow=true;
        if(mode==='hidden')x.page.hidden=true;
        if(mode==='empty')x.heading.title='';
        initializePageHeadings(f.doc);
        assert.equal(x.overlay.hasAttribute('data-f-heading-enhanced'),false,mode);
        assert.equal(visible(x.overlay),false);
    }
});
test('independent scroll regions have independent titles without stacking sample headings in one region',()=>{
    const f=fixture(),a=f.add(),sample=f.add(a.stage),b=f.add(f.region(1050,100,100,600));
    a.stage.scrollTop=300;b.stage.scrollTop=0;initializePageHeadings(f.doc);
    assert.equal(visible(a.overlay),true);assert.equal(visible(sample.overlay),false);assert.equal(visible(b.overlay),false);
    b.stage.scrollTop=300;f.scroll();assert.equal(visible(b.overlay),true);
});
test('route replacement releases retired geometry; removal cancels observers, listeners and pending work',()=>{
    const f=fixture(),a=f.add();a.stage.scrollTop=300;initializePageHeadings(f.doc);
    const before=f.observers.length;initializePageHeadings(f.doc);f.flush();assert.equal(f.observers.length,before);
    f.headings.length=0;const b=f.add();f.changed();
    assert.equal(a.overlay.hasAttribute('data-f-heading-enhanced'),false);
    assert.equal(f.observers[0].targets.has(a.heading),false);assert.equal(f.observers[0].targets.has(b.heading),true);
    f.headings.length=0;f.changed();assert.equal(f.events.size,0);assert.equal(f.frames.size,0);
    assert.ok(f.observers.every(observer=>observer.closed));
    f.add();initializePageHeadings(f.doc);assert.ok(f.events.size>0);
});
test('live controls keep one instance and reserve normal geometry while their fixed positions remain stable',()=>{
    const f=fixture(),x=f.add(),controls=f.controls(x),[parent,actions]=controls;
    const button=parent.content,action=actions.content;
    x.stage.scrollTop=300;initializePageHeadings(f.doc);
    assert.equal(parent.anchor.hasAttribute('data-f-heading-fixed'),true);
    assert.equal(parent.slot.style.getPropertyValue('--f-heading-slot-width'),'130px');
    assert.equal(parent.slot.style.getPropertyValue('--f-heading-slot-height'),'56px');
    assert.equal(parent.anchor.style.getPropertyValue('--f-heading-control-width'),'48px');
    assert.equal(actions.anchor.style.getPropertyValue('--f-heading-control-left'),'736px');
    assert.equal(parent.content,button);assert.equal(actions.content,action);
    assert.equal(x.stage.scrollHeight,1900);assert.equal(x.heading.height,174);
    const writes=f.writes(),refresh=x.heading.popupRefreshes;
    f.scroll();assert.equal(f.writes(),writes);assert.equal(x.heading.popupRefreshes,refresh);
    x.stage.scrollTop=0;f.scroll();
    assert.equal(parent.anchor.hasAttribute('data-f-heading-fixed'),false);
    assert.equal(parent.slot.hasAttribute('data-f-heading-reserved'),false);
    assert.equal(actions.content,action);assert.ok(x.heading.popupRefreshes>refresh);
});
test('focused controls remain visible across dirty measurement and release when their natural slot returns',()=>{
    const f=fixture(),x=f.add(),[parent,actions]=f.controls(x);
    x.stage.scrollTop=300;initializePageHeadings(f.doc);f.doc.activeElement=parent.content;
    x.stage.scrollTop=100;f.scroll();assert.equal(visible(x.overlay),true);
    actions.content.naturalHeight=80;f.changed();
    assert.equal(visible(x.overlay),true);assert.equal(parent.anchor.hasAttribute('data-f-heading-fixed'),true);
    assert.equal(actions.slot.style.getPropertyValue('--f-heading-slot-height'),'80px');
    assert.equal(f.doc.activeElement,parent.content);
    x.stage.scrollTop=0;f.scroll();assert.equal(visible(x.overlay),false);
    assert.equal(f.doc.activeElement,parent.content);
});
test('asynchronous action changes remeasure reserved slots, hidden controls release and narrow actions stack',()=>{
    const f=fixture(),x=f.add(f.region(10,100,300,600)),[parent,actions]=f.controls(x);
    x.stage.scrollTop=300;initializePageHeadings(f.doc);
    assert.equal(x.overlay.hasAttribute('data-f-heading-stacked'),true);
    assert.equal(actions.anchor.style.getPropertyValue('--f-heading-control-top'),'172px');
    actions.content.hidden=true;f.changed();
    assert.equal(actions.anchor.hasAttribute('data-f-heading-fixed'),false);
    assert.equal(actions.space.hidden,true);
    actions.content.hidden=false;actions.content.naturalWidth=400;actions.content.naturalHeight=64;f.changed();
    assert.equal(actions.anchor.hasAttribute('data-f-heading-fixed'),true);
    assert.equal(actions.slot.style.getPropertyValue('--f-heading-slot-width'),'400px');
    assert.equal(actions.slot.style.getPropertyValue('--f-heading-slot-height'),'64px');
    assert.ok(f.observers[0].targets.has(actions.content));
    f.headings.length=0;f.changed();
    assert.equal(parent.anchor.hasAttribute('data-f-heading-fixed'),false);
    assert.equal(actions.slot.hasAttribute('data-f-heading-reserved'),false);
});
test('SSR owns one inert text-only overlay; enhanced motion never changes original heading dimensions',async()=>{
    const razor=await readFile(new URL('../Components/PageHeading.razor',assets),'utf8');
    assert.equal((razor.match(/<h1>/g)??[]).length,1);
    assert.match(razor,/@if \(!Compact\)[\s\S]*data-f-heading-overlay aria-hidden="true" inert/);
    const overlay=razor.slice(razor.indexOf('<div class="f-heading-overlay"'),razor.indexOf('</header>'));
    assert.doesNotMatch(overlay,/@Actions|Href="@ParentHref"|<h1|tabindex|\bid=/);
    assert.match(overlay,/@Title/);
    const css=await readFile(new URL('framework.css',assets),'utf8');
    assert.match(css,/\.f-heading-overlay\[data-f-heading-overlay\][^}]+position:fixed[^}]+height:0/);
    assert.match(css,/\.f-heading-overlay-panel[^}]+translateY\(-100%\)/);
    assert.match(css,/data-f-heading-visible[^}]+translateY\(0\)/);
    assert.match(css,/@media\(prefers-reduced-motion:no-preference\)[\s\S]*transition:transform \.18s/);
    assert.doesNotMatch(css,/data-compact-heading/);
    const bootstrap=await readFile(new URL('primitives/input-behaviors.js',assets),'utf8');
    assert.match(bootstrap,/import\('\.\.\/patterns\/page-heading\.js'\)/);
    assert.match(bootstrap,/initializePageHeadings\(document\)/);
});

test('shell measurement requires and owns the native ResizeObserver lifecycle', () => {
    const previous = { window: globalThis.window, ResizeObserver: globalThis.ResizeObserver };
    const observers = [], properties = new Map();
    const titlebar = { height: 68, getBoundingClientRect() { return { height: this.height }; } };
    const primary = { width: 80, getBoundingClientRect() { return { width: this.width }; } };
    const root = { attrs: new Set(), dataset: {}, style: { setProperty: (key, value) => properties.set(key, value) },
        querySelector: selector => selector === '.f-titlebar' ? titlebar : selector === '.f-primary-navigation' ? primary : null,
        querySelectorAll: () => [], addEventListener() {}, removeEventListener() {},
        hasAttribute(name) { return this.attrs.has(name); }, removeAttribute(name) { this.attrs.delete(name); } };
    const content = { scrollTop: 0, addEventListener() {}, removeEventListener() {} };
    globalThis.window = { addEventListener() {}, removeEventListener() {} };
    globalThis.ResizeObserver = class {
        constructor(callback) { this.callback = callback; this.observed = []; observers.push(this); }
        observe(node) { this.observed.push(node); }
        disconnect() { this.disconnected = true; }
    };
    try {
        attach(root, content);
        assert.deepEqual(observers[0].observed, [titlebar, primary]);
        assert.equal(properties.get('--f-shell-titlebar-size'), '68px');
        titlebar.height = 96; observers[0].callback();
        assert.equal(properties.get('--f-shell-titlebar-size'), '96px');
        detach(root);
        assert.equal(observers[0].disconnected, true);
        delete globalThis.ResizeObserver;
        assert.throws(() => attach(root, content), ReferenceError,
            'An unsupported browser must fail instead of silently disabling live shell measurement.');
    } finally {
        globalThis.window = previous.window;
        if (previous.ResizeObserver === undefined) delete globalThis.ResizeObserver;
        else globalThis.ResizeObserver = previous.ResizeObserver;
    }
});

test('production interaction sources do not select alternative layouts when required modern APIs are absent', async () => {
    for (const name of ['shell.js', 'primitives/primary-navigation-item.js', 'patterns/surfaces.js',
        'section-navigator.js', 'primitives/editing-grid-columns.js', 'primitives/editing-grid.js',
        'presentation/offers.js', 'data.js']) {
        const text = await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/' + name, import.meta.url), 'utf8');
        assert.doesNotMatch(text, /typeof[^;\n]*(?:ResizeObserver|MutationObserver|requestAnimationFrame|showPopover|hidePopover)/, name);
        assert.doesNotMatch(text, /if\s*\(!window\?\.matchMedia\)/, name);
        assert.doesNotMatch(text, /runtime without observers|\.f-page-heading, \.page-heading/, name);
    }
});
