const documents = new WeakMap();
const selector = '.f-uniform-grid,.f-form-actions';
const epsilon = 1 / 64;
const properties = ['images','positions','sizes'];
const pixels = value => Math.round(value * 1000) / 1000 + 'px';
const number = value => Number.parseFloat(value) || 0;

// Physical intersections also cover RTL, column-first placement and partial shared edges.
export function sharedEdges(rectangles) {
    const cells = rectangles.filter(rect => rect.right > rect.left && rect.bottom > rect.top);
    const left = new Map(), top = new Map(), segments = [];
    const key = value => Math.round(value / epsilon);
    const add = (map,value,rect) => {
        const bucket = key(value);
        if (!map.has(bucket)) map.set(bucket,[]);
        map.get(bucket).push(rect);
    };
    for (const cell of cells) { add(left,cell.left,cell); add(top,cell.top,cell); }
    for (const cell of cells) for (const [axis,map,end,start,low,high] of [
        ['vertical',left,'right','left','top','bottom'],['horizontal',top,'bottom','top','left','right']]) {
        const bucket = key(cell[end]);
        for (let offset=-1;offset<=1;offset++) for (const neighbor of map.get(bucket+offset) ?? []) {
            if (neighbor === cell || Math.abs(cell[end]-neighbor[start]) > epsilon) continue;
            const from = Math.max(cell[low],neighbor[low]), to = Math.min(cell[high],neighbor[high]);
            if (to-from > epsilon) segments.push({axis,at:(cell[end]+neighbor[start])/2,from,to});
        }
    }
    segments.sort((a,b) => a.axis.localeCompare(b.axis) || a.at-b.at || a.from-b.from);
    const merged = [];
    for (const segment of segments) {
        const last = merged.at(-1);
        if (last && last.axis === segment.axis && Math.abs(last.at-segment.at)<=epsilon && segment.from<=last.to+epsilon)
            last.to=Math.max(last.to,segment.to);
        else merged.push({...segment});
    }
    return merged;
}
function release(state,group) {
    group.removeAttribute('data-f-grid-dividers');
    for (const property of properties) group.style.removeProperty('--f-grid-divider-'+property);
    state.ownedStyles.set(group,group.getAttribute('style'));
}
function extent(style,axis,fallback) {
    const width = number(style[axis]);
    if (!width) return fallback;
    if (style.boxSizing === 'border-box') return width;
    return width + (axis === 'width'
        ? number(style.paddingLeft)+number(style.paddingRight)+number(style.borderLeftWidth)+number(style.borderRightWidth)
        : number(style.paddingTop)+number(style.paddingBottom)+number(style.borderTopWidth)+number(style.borderBottomWidth));
}
function update(state) {
    state.frame=0;
    const groups=new Set(state.doc.querySelectorAll(selector)), observed=new Set();
    for (const group of groups) {
        observed.add(group);
        const cells=[...group.children].filter(cell => cell.matches('.f-uniform-cell'));
        cells.forEach(cell => observed.add(cell));
        const style=state.win.getComputedStyle(group), box=group.getBoundingClientRect();
        if (!group.getClientRects().length || group.closest('[hidden],dialog:not([open])')
            || style.visibility!=='visible' || box.width<=0 || box.height<=0) { release(state,group); continue; }
        const scaleX=box.width/extent(style,'width',box.width), scaleY=box.height/extent(style,'height',box.height);
        const originX=box.left+number(style.borderLeftWidth)*scaleX, originY=box.top+number(style.borderTopWidth)*scaleY;
        const rectangles=cells.filter(cell => cell.getClientRects().length && !cell.hidden
            && state.win.getComputedStyle(cell).visibility==='visible').map(cell => {
            const rect=cell.getBoundingClientRect();
            return {left:(rect.left-originX)/scaleX,right:(rect.right-originX)/scaleX,
                top:(rect.top-originY)/scaleY,bottom:(rect.bottom-originY)/scaleY};
        });
        const segments=sharedEdges(rectangles);
        if (!segments.length) { release(state,group); continue; }
        const values={images:[],positions:[],sizes:[]};
        for (const edge of segments) {
            values.images.push('linear-gradient(var(--f-grid-divider-color,transparent),var(--f-grid-divider-color,transparent))');
            values.positions.push(edge.axis==='vertical' ? pixels(edge.at-.5)+' '+pixels(edge.from) : pixels(edge.from)+' '+pixels(edge.at-.5));
            values.sizes.push(edge.axis==='vertical' ? '1px '+pixels(edge.to-edge.from) : pixels(edge.to-edge.from)+' 1px');
        }
        for (const property of properties) {
            const key='--f-grid-divider-'+property, value=values[property].join(',');
            if (group.style.getPropertyValue(key)!==value) group.style.setProperty(key,value);
        }
        if (!group.hasAttribute('data-f-grid-dividers')) group.setAttribute('data-f-grid-dividers','');
        state.ownedStyles.set(group,group.getAttribute('style'));
    }
    for (const group of state.groups) if (!groups.has(group)) release(state,group);
    for (const node of state.observed) if (!observed.has(node)) state.resize.unobserve(node);
    for (const node of observed) if (!state.observed.has(node)) state.resize.observe(node);
    state.groups=groups; state.observed=observed;
    if (!groups.size) {
        state.disposed=true;
        if(state.frame) state.win.cancelAnimationFrame(state.frame);
        state.resize.disconnect();state.mutation.disconnect();
        state.removers.forEach(remove=>remove());
        documents.delete(state.doc);
    }
}
export function initializeGridDividers(root=document) {
    const doc=root.ownerDocument ?? root;
    let state=documents.get(doc);
    if(state) { state.schedule();return; }
    if(!doc.querySelector(selector)) return;
    const win=doc.defaultView;
    state={doc,win,groups:new Set(),observed:new Set(),ownedStyles:new WeakMap(),frame:0,removers:[]};
    state.schedule=()=>{if(!state.disposed&&!state.frame)state.frame=win.requestAnimationFrame(()=>update(state));};
    state.resize=new win.ResizeObserver(state.schedule);
    state.mutation=new win.MutationObserver(records=>{
        const relevant = node => [...state.groups].some(group => group.contains(node) || node.contains?.(group));
        if(records.some(record => {
            if(record.type === 'attributes' && record.attributeName === 'style'
                && state.ownedStyles.has(record.target) && state.ownedStyles.get(record.target)===record.target.getAttribute('style')) return false;
            if(record.target.matches?.(selector) || relevant(record.target)) return true;
            return record.type==='childList' && [...record.addedNodes,...record.removedNodes].some(node =>
                node.matches?.(selector) || node.querySelector?.(selector));
        })) state.schedule();
    });
    state.mutation.observe(doc.documentElement,{childList:true,subtree:true,characterData:true,attributes:true,
        attributeFilter:['hidden','class','style','dir','open']});
    const listen=(target,type)=>{
        target.addEventListener(type,state.schedule);
        state.removers.push(()=>target.removeEventListener(type,state.schedule));
    };
    listen(win,'resize');listen(doc,'enhancedload');listen(doc,'transitionend');listen(doc,'animationend');
    documents.set(doc,state);
    update(state);
}
