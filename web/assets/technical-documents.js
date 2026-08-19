document.addEventListener("DOMContentLoaded",()=>{
  const search=document.getElementById("docsSearch");
  const type=document.getElementById("docsType");
  const language=document.getElementById("docsLanguage");
  const cards=[...document.querySelectorAll(".docs-card")];
  const count=document.querySelector(".docs-count");
  const empty=document.querySelector(".docs-empty");
  if(!cards.length)return;
  const render=()=>{
    const query=(search?.value||"").toLocaleLowerCase("tr-TR").trim();
    let visible=0;
    cards.forEach(card=>{
      const matches=(!query||card.textContent.toLocaleLowerCase("tr-TR").includes(query))&&(!type?.value||card.dataset.type===type.value)&&(!language?.value||card.dataset.language===language.value);
      card.hidden=!matches;
      if(matches)visible+=1;
    });
    if(count)count.textContent=`${visible} doküman`;
    if(empty)empty.hidden=visible!==0;
  };
  search?.addEventListener("input",render);
  type?.addEventListener("change",render);
  language?.addEventListener("change",render);
  render();
});
