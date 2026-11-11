import assert from 'node:assert/strict';
import test from 'node:test';
import {readFile} from 'node:fs/promises';
const assets=new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/',import.meta.url);
const code=await readFile(new URL('patterns/uniform-grid-dividers.js',assets),'utf8');
const {sharedEdges,initializeGridDividers}=await import('data:text/javascript;base64,'+Buffer.from(code).toString('base64'));
const rect=(left,top,width=100,height=80)=>({left,top,right:left+width,bottom:top+height,width,height});
const edge=(axis,at,from,to)=>({axis,at,from,to});
test('empty and standalone cells have no edge and corners or real gaps do not count as adjacency',()=>{
    for(const cells of [[],[rect(0,0)],[rect(0,0),rect(101,0)],[rect(0,0),rect(100,80)]])
        assert.deepEqual(sharedEdges(cells),[]);
});
test('row and column-first incomplete layouts draw only occupied shared boundaries',()=>{
    const cells=[rect(0,0),rect(100,0),rect(200,0),rect(0,80),rect(100,80)];
    const expected=[edge('horizontal',80,0,200),edge('vertical',100,0,160),edge('vertical',200,0,80)];
    assert.deepEqual(sharedEdges(cells),expected);
    assert.deepEqual(sharedEdges([cells[0],cells[3],cells[1],cells[4],cells[2]]),expected);
    assert.deepEqual(sharedEdges([...cells].reverse()),expected,'RTL/order does not change physical intersections');
});
test('partial overlaps and wide spanning tracks never draw the uncovered portion of an edge',()=>{
    assert.deepEqual(sharedEdges([rect(0,0,200,180),rect(200,0,100,80)]),[edge('vertical',200,0,80)]);
    assert.deepEqual(sharedEdges([rect(0,0,200,80),rect(0,80,100,80)]),[edge('horizontal',80,0,100)]);
    assert.deepEqual(sharedEdges([rect(0,0,100,160),rect(100,0),rect(100,80)]),
        [edge('horizontal',80,100,200),edge('vertical',100,0,160)]);
});
test('fractional tracks merge without widening gaps or manufacturing exterior lines',()=>{
    const cells=[rect(0,0,100.25,80),rect(100.25,0,100.25,80)];
    assert.deepEqual(sharedEdges(cells),[edge('vertical',100.25,0,80)]);
    assert.deepEqual(sharedEdges([cells[0],rect(100.3,0,100.25,80)]),[]);
});
function fixture() {
    const groups=[],observers=[],frames=new Map(),events=new Map();let next=0,writes=0;
    const eventTarget=(name)=>({addEventListener(type,fn){events.set(name+type,fn);},removeEventListener(type){events.delete(name+type);}});
    class Observer{
        constructor(callback){this.callback=callback;this.targets=new Set();observers.push(this);}
        observe(node){this.targets.add(node);}unobserve(node){this.targets.delete(node);}
        disconnect(){this.closed=true;this.targets.clear();}
    }
    const win={...eventTarget('window'),ResizeObserver:Observer,MutationObserver:Observer,
        requestAnimationFrame(fn){frames.set(++next,fn);return next;},cancelAnimationFrame(id){frames.delete(id);},
        getComputedStyle:node=>({visibility:'visible',boxSizing:'border-box',...node.computed})};
    const doc={...eventTarget('doc'),defaultView:win,querySelector:()=>groups[0],querySelectorAll:()=>groups};
    function node(parent=null){
        const attrs=new Map(),styles=new Map();
        return {ownerDocument:doc,parentElement:parent,children:[],hidden:false,computed:{},
            contains(other){for(let n=other;n;n=n.parentElement)if(n===this)return true;return false;},
            matches(selector){return selector==='.f-uniform-cell'?this.cell:!!this.group;},querySelector:()=>null,
            getAttribute:name=>name==='style'?[...styles].map(([key,value])=>key+':'+value).join(';'):attrs.get(name)??null,
            hasAttribute:name=>attrs.has(name),setAttribute(name,value=''){if(attrs.get(name)!==value){attrs.set(name,value);writes++;}},
            removeAttribute(name){if(attrs.delete(name))writes++;},
            style:{getPropertyValue:name=>styles.get(name)??'',setProperty(name,value){styles.set(name,value);writes++;},removeProperty(name){if(styles.delete(name))writes++;}},
            getClientRects(){return this.hidden?[]:[this.box];},getBoundingClientRect(){return this.box;},
            closest(){for(let n=this;n;n=n.parentElement)if(n.hidden)return n;return null;}
        };
    }
    doc.documentElement=node();
    function group(cells=[rect(0,0),rect(100,0)],scale=1,width=200,height=80) {
        const group=node(doc.documentElement);group.group=true;group.computed={width:width+'px',height:height+'px'};
        group.box=rect(20,30,width*scale,height*scale);
        group.children=cells.map(box=>{
            const cell=node(group);cell.cell=true;cell.box=rect(20+box.left*scale,30+box.top*scale,box.width*scale,box.height*scale);return cell;
        });
        groups.push(group);return group;
    }
    function flush(){let count=0;while(frames.size){const all=[...frames.values()];frames.clear();all.forEach(fn=>fn());assert.ok(++count<5,'unstable frame loop');}}
    return {doc,win,groups,group,node,observers,frames,events,flush,writes:()=>writes,
        resize(){observers.find(o=>!o.closed)?.callback([]);flush();},
        mutate(records){observers.find(o=>!o.closed&&o.targets.has(doc.documentElement))?.callback(records);flush();}};
}
test('scaled and fractional groups paint in local CSS pixels rather than background-position percentages',()=>{
    for(const scale of [1,1.25,2]){
        const f=fixture(),g=f.group([rect(0,0,100.25,80),rect(100.25,0,100.25,80)],scale,200.5);
        initializeGridDividers(f.doc);
        assert.equal(g.style.getPropertyValue('--f-grid-divider-positions'),'99.75px 0px');
        assert.equal(g.style.getPropertyValue('--f-grid-divider-sizes'),'1px 80px');
        assert.equal(g.hasAttribute('data-f-grid-dividers'),true);
    }
});
test('resize and narrow-column relayout recalculate geometry while plain non-cell children never draw boundaries',()=>{
    const f=fixture(),g=f.group();initializeGridDividers(f.doc);
    const avatar=f.node(g);avatar.box=rect(220,30);g.children.push(avatar);
    g.children[1].box=rect(20,110);g.box=rect(20,30,100,160);g.computed={width:'100px',height:'160px'};
    f.resize();
    assert.equal(g.style.getPropertyValue('--f-grid-divider-positions'),'0px 79.5px');
    assert.equal(g.style.getPropertyValue('--f-grid-divider-sizes'),'100px 1px');
    g.children[1].hidden=true;f.resize();
    assert.equal(g.hasAttribute('data-f-grid-dividers'),false);
    g.children[1].hidden=false;f.resize();assert.equal(g.hasAttribute('data-f-grid-dividers'),true);
});
test('owned paint writes and unrelated heading styles never schedule repeated geometry work',()=>{
    const f=fixture(),g=f.group();initializeGridDividers(f.doc);
    const writes=f.writes();f.resize();assert.equal(f.writes(),writes);
    const observer=f.observers.find(o=>o.targets.has(f.doc.documentElement));
    observer.callback([{type:'attributes',attributeName:'style',target:g}]);assert.equal(f.frames.size,0);
    const unrelated=f.node(f.doc.documentElement);
    observer.callback([{type:'attributes',attributeName:'style',target:unrelated}]);assert.equal(f.frames.size,0);
    g.style.setProperty('direction','rtl');
    observer.callback([{type:'attributes',attributeName:'style',target:g}]);assert.equal(f.frames.size,1);f.flush();
    observer.callback([{type:'attributes',attributeName:'class',target:f.doc.documentElement}]);assert.equal(f.frames.size,1);f.flush();
});
test('hidden groups release paint, replacement releases observations and final unmount disposes the document controller',()=>{
    const f=fixture(),g=f.group();initializeGridDividers(f.doc);
    g.hidden=true;f.resize();assert.equal(g.hasAttribute('data-f-grid-dividers'),false);
    g.hidden=false;f.resize();assert.equal(g.hasAttribute('data-f-grid-dividers'),true);
    f.groups.length=0;
    f.mutate([{type:'childList',target:f.doc.documentElement,addedNodes:[],removedNodes:[g]}]);
    assert.equal(g.style.getPropertyValue('--f-grid-divider-images'),'');
    assert.ok(f.observers.every(o=>o.closed));assert.equal(f.events.size,0);assert.equal(f.frames.size,0);
    f.group();initializeGridDividers(f.doc);assert.ok(f.events.size>0);
});
test('Framework supplies only a pointer-transparent out-of-flow layer; Design owns color and forced-colors paint',async()=>{
    const css=await readFile(new URL('uniform-grid.css',assets),'utf8');
    const skin=await readFile(new URL('../../Flourish.Blazor.Design/wwwroot/uniform-grid.css',assets),'utf8');
    assert.match(css,/\[data-f-grid-dividers\]::after\s*\{[^}]*position:absolute[^}]*pointer-events:none/s);
    assert.match(css,/background-repeat:no-repeat/);
    assert.match(skin,/border:0 solid transparent/);
    assert.match(skin,/--f-grid-gap:0/);
    assert.match(skin,/@media\(forced-colors:active\)\s*\{\s*\.f-uniform-grid,\.f-form-actions\s*\{\s*--f-grid-divider-color:CanvasText/);
    assert.match(skin,/\[data-f-grid-dividers\]::after\s*\{\s*forced-color-adjust:none/);
    assert.doesNotMatch(skin,/border-color:var\(--f-border\)/);
    const bootstrap=await readFile(new URL('primitives/input-behaviors.js',assets),'utf8');
    assert.match(bootstrap,/import\('\.\.\/patterns\/uniform-grid-dividers\.js'\)/);
    assert.match(bootstrap,/initializeGridDividers\(document\)/);
    const gallery=await readFile(new URL('../../../Gallery.Flourish.Blazor/Components/App.razor',assets),'utf8');
    assert.match(gallery,/type="module" src="@Assets\["_content\/Arkheide.Flourish.Blazor.Framework\/primitives\/input-behaviors\.js"\]"/);
});
