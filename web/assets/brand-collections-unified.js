(() => {
  const stoneGrid=document.getElementById('stoneListingGrid');
  const kutahyaGrid=document.querySelector('main .listing .grid');
  const grid=stoneGrid||kutahyaGrid;
  if(!grid) return;
  const cards=[...grid.children].filter(card=>card.matches('.stone-listing-card,.card'));
  const heading=grid.closest('.wrap')?.querySelector('.section-heading,.heading');
  if(!heading) return;
  const oldPagination=document.getElementById('stoneListingPagination');
  if(oldPagination) oldPagination.remove();
  cards.forEach(card=>card.hidden=false);
  const names=cards.map(card=>card.querySelector('h2,h3')?.textContent.trim()||'Koleksiyon');
  const categoryFor=(name,index)=>cardCategory(cards[index],name);
  function cardCategory(card,name){
    if(card.dataset.category) return card.dataset.category;
    const text=(name+' '+(card.querySelector('p')?.textContent||'')).toLocaleLowerCase('tr-TR');
    if(/oak|ahşap/.test(text))return'Ahşap';if(/cement|beton/.test(text))return'Cement';
    if(/onyx|oniks/.test(text))return'Oniks';if(/taş|stone|basalt|atlantis/.test(text))return'Taş';
    return'Mermer';
  }
  const categories=[];
  cards.forEach((card,index)=>{card.dataset.name=card.dataset.name||names[index];card.dataset.category=categories[index%categories.length]});
  cards.sort((a,b)=>a.dataset.name.localeCompare(b.dataset.name,'tr',{sensitivity:'base'})).forEach(card=>grid.appendChild(card));
  const controls=document.createElement('div');controls.className='unified-collections-controls';
  controls.innerHTML='<div class="unified-collections-filters" role="group" aria-label="Kategori filtreleri"><button class="unified-collections-filter" data-filter="all" aria-pressed="true">Tümü</button>'+categories.map(c=>'<button class="unified-collections-filter" data-filter="'+c+'" aria-pressed="false">'+c+'</button>').join('')+'</div><input class="unified-collections-search" type="search" aria-label="Koleksiyon ara" placeholder="Koleksiyon ara">';
  heading.insertAdjacentElement('afterend',controls);
  const empty=document.createElement('p');empty.className='unified-collections-empty';empty.hidden=true;empty.textContent='Aramanızla eşleşen koleksiyon bulunamadı.';grid.insertAdjacentElement('afterend',empty);
  const pagination=document.createElement('nav');pagination.className='unified-listing-pagination';pagination.setAttribute('aria-label','Sayfalama');
  pagination.innerHTML='<button class="unified-listing-page-btn" type="button" aria-label="Önceki sayfa">←</button><div class="unified-listing-page-numbers"></div><button class="unified-listing-page-btn" type="button" aria-label="Sonraki sayfa">→</button>';
  empty.insertAdjacentElement('afterend',pagination);
  let active='all',currentPage=1;const pageSize=4,search=controls.querySelector('input'),buttons=[...controls.querySelectorAll('button')],pageButtons=pagination.querySelectorAll('.unified-listing-page-btn'),pageNumbers=pagination.querySelector('.unified-listing-page-numbers');
  const getMatches=()=>{const q=search.value.toLocaleLowerCase('tr-TR').trim();return cards.filter(card=>(active==='all'||card.dataset.category===active)&&(!q||card.dataset.name.toLocaleLowerCase('tr-TR').includes(q)))};
  const render=()=>{const matched=getMatches(),totalPages=Math.max(1,Math.ceil(matched.length/pageSize));currentPage=Math.min(currentPage,totalPages);cards.forEach(card=>card.hidden=true);matched.slice((currentPage-1)*pageSize,currentPage*pageSize).forEach(card=>card.hidden=false);empty.hidden=!!matched.length;pagination.hidden=matched.length===0||totalPages<=1;pageNumbers.innerHTML='';for(let i=1;i<=totalPages;i++){const button=document.createElement('button');button.type='button';button.textContent=String(i);button.classList.toggle('active',i===currentPage);button.onclick=()=>{currentPage=i;render()};pageNumbers.appendChild(button)}pageButtons[0].disabled=currentPage===1;pageButtons[1].disabled=currentPage===totalPages};
  buttons.forEach(button=>button.onclick=()=>{active=button.dataset.filter;currentPage=1;buttons.forEach(b=>b.setAttribute('aria-pressed',String(b===button)));render()});search.oninput=()=>{currentPage=1;render()};pageButtons[0].onclick=()=>{if(currentPage>1){currentPage--;render()}};pageButtons[1].onclick=()=>{if(currentPage<Math.ceil(getMatches().length/pageSize)){currentPage++;render()}};render();
})();
