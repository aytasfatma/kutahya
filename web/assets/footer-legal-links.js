document.addEventListener("DOMContentLoaded",()=>{
  const targets={Gizlilik:"kvkk-gizlilik.html#gizlilik",Çerezler:"cerez-politikasi.html",KVKK:"kvkk-gizlilik.html#kvkk"};
  document.querySelectorAll(".footer__legal a").forEach(link=>{const target=targets[link.textContent.trim()];if(target)link.href=target;});
  const navTargets={"Satış Noktaları":"satis-noktalari.html"};
  document.querySelectorAll("header a,.mobile-menu a,.side-menu a").forEach(link=>{const target=navTargets[link.textContent.trim()];if(target)link.href=target;});
  document.querySelectorAll('a[href="showroom-detay.html"]').forEach(link=>{link.href="bayi-detay.html";});
});
document.addEventListener("click",event=>{const link=event.target.closest(".footer__legal a");if(link?.textContent.trim()==="Yasal Uyarı")event.preventDefault();const showroom=event.target.closest("header a,.mobile-menu a,.side-menu a");if(showroom?.textContent.trim()==="Showroomlar")event.preventDefault();});
