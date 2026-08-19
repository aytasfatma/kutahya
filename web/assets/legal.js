document.addEventListener("DOMContentLoaded",()=>{
  const footer=document.querySelector(".footer.footer--dark");
  if(footer)footer.innerHTML=`<div class="wrap">
    <div class="footer__main-final">
      <div class="footer__identity-final"><div class="footer__info"><img alt="NG Kütahya Seramik" class="logo-img logo-img--footer" src="assets/ng-kutahya-logo.png"></div></div>
      <div class="footer__menus-final">
        <div class="footer__col"><h4>Kurumsal</h4><a href="hakkimizda.html#hakkimizda">Hakkımızda</a><a href="hakkimizda.html#tarihce">Tarihçe</a><a href="hakkimizda.html#uretim">Üretim</a><a href="hakkimizda.html#degerlerimiz">Temel Değerlerimiz</a><a href="hakkimizda.html#odullar">Başarılar / Ödüller</a><a href="hakkimizda.html#sertifikalar">Sertifikalar</a><a href="hakkimizda.html#isbirlikleri">İş Birlikleri</a><a href="kariyer.html">Kariyer</a><a href="hakkimizda.html#bilgitoplumu">Bilgi Toplumu Hizmetleri</a></div>
        <div class="footer__col"><h4>Sürdürülebilirlik</h4><a href="#">Çevresel Sorumluluk</a><a href="#">Sürdürülebilirlik Raporu</a></div>
        <div class="footer__col"><h4>Katalog ve Belgeler</h4><a href="#">Ürün Katalogları</a><a href="hakkimizda.html#sertifikalar">Sertifikalar</a><a href="teknik-dokumanlar.html">Teknik Dokümanlar</a></div>
        <div class="footer__col"><h4>Fuar</h4><a href="#">Fuarlar</a><a href="#">Sanal Tur</a></div>
        <div class="footer__col"><h4>Medya</h4><a href="#">NG Dergi</a><a href="haberler.html">Bültenler</a><a href="#">Trendler</a><a href="#">Videolar</a><a href="blog.html">Blog Yazıları</a></div>
        <div class="footer__col"><h4>Grup Şirketleri</h4><a href="#">Kütahya Porselen</a><a href="#">NG Hotels</a><a href="#">NG Residence</a><a href="#">NG Yatırım</a><a href="#">NG Makine</a><a href="#">NG Lojistik</a></div>
        <div class="footer__col"><h4>Global Şirketler</h4><a href="#">NG USA</a><a href="#">NG Arteka</a></div>
      </div>
    </div>
    <div class="footer__social-row"><div aria-label="Sosyal medya" class="footer__socials footer__socials--bottom">
      <a aria-label="Instagram" href="#" title="Instagram"><svg aria-hidden="true" viewBox="0 0 24 24"><rect height="17" rx="4" width="17" x="3.5" y="3.5"></rect><circle cx="12" cy="12" r="4"></circle><circle cx="17.5" cy="6.5" r="1"></circle></svg></a>
      <a aria-label="Facebook" href="#" title="Facebook"><svg aria-hidden="true" viewBox="0 0 24 24"><path d="M14 8h3V4h-3c-3 0-5 2-5 5v3H6v4h3v5h4v-5h3l1-4h-4V9c0-.7.3-1 1-1Z"></path></svg></a>
      <a aria-label="YouTube" href="#" title="YouTube"><svg aria-hidden="true" viewBox="0 0 24 24"><rect height="12" rx="3" width="19" x="2.5" y="6"></rect><path d="m10 9 5 3-5 3Z"></path></svg></a>
      <a aria-label="LinkedIn" href="#" title="LinkedIn"><svg aria-hidden="true" viewBox="0 0 24 24"><rect height="11" width="4" x="4" y="9"></rect><circle cx="6" cy="5.5" r="2"></circle><path d="M12 20V9h4v1.7c.9-1.3 2.1-2 3.6-2 2.9 0 4.4 1.8 4.4 5.4V20h-4v-5.2c0-1.7-.6-2.6-1.9-2.6-1.4 0-2.1 1-2.1 3V20Z"></path></svg></a>
    </div></div>
    <div class="footer__bottom"><p class="footer__copy">© 2026 NG Kütahya Seramik. Tüm hakları saklıdır.</p><div class="footer__bottom-right footer__bottom-right--legal-only"><div class="footer__legal"><a href="kvkk-gizlilik.html#gizlilik">Gizlilik</a><a href="cerez-politikasi.html">Çerezler</a><a href="kvkk-gizlilik.html#kvkk">KVKK</a><a href="#">Yasal Uyarı</a></div></div></div>
    <div class="footer__signature">Digital Experience by Most Idea</div>
  </div>`;
  document.querySelectorAll(".footer__col").forEach((column) => {
    const heading = column.querySelector("h4");
    if (heading?.textContent.trim() !== "Kurumsal" || heading.querySelector(".footer__heading-link")) return;
    const headingLink = document.createElement("a");
    headingLink.className = "footer__heading-link";
    headingLink.href = "hakkimizda.html";
    headingLink.textContent = heading.textContent.trim();
    heading.replaceChildren(headingLink);
  });
  const header=document.getElementById("header");
  const sync=()=>header?.classList.toggle("scrolled",scrollY>30);
  addEventListener("scroll",sync,{passive:true});sync();
});
