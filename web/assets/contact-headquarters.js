document.addEventListener("DOMContentLoaded",()=>{
  const routeButton=document.querySelector("[data-headquarters-route]");
  const status=document.querySelector("[data-route-status]");
  if(!routeButton)return;
  const openRoute=(origin="",routeWindow=null)=>{
    const destination=routeButton.dataset.destination||"Çalca Mahallesi, 43001 Kütahya, Türkiye";
    const parameters=new URLSearchParams({api:"1",destination,travelmode:"driving"});
    if(origin)parameters.set("origin",origin);
    const routeUrl=`https://www.google.com/maps/dir/?${parameters.toString()}`;
    if(routeWindow){routeWindow.opener=null;routeWindow.location.replace(routeUrl);}
    else window.location.href=routeUrl;
  };
  routeButton.addEventListener("click",()=>{
    const routeWindow=window.open("about:blank","_blank");
    if(!navigator.geolocation){
      if(status)status.textContent="Konum bilgisi desteklenmiyor; hedef adres Google Maps'te açılıyor.";
      openRoute("",routeWindow);
      return;
    }
    routeButton.disabled=true;
    if(status)status.textContent="Bulunduğunuz konum belirleniyor…";
    navigator.geolocation.getCurrentPosition(
      position=>{
        routeButton.disabled=false;
        if(status)status.textContent="Rota yeni sekmede açıldı.";
        openRoute(`${position.coords.latitude},${position.coords.longitude}`,routeWindow);
      },
      ()=>{
        routeButton.disabled=false;
        if(status)status.textContent="Konum izni alınamadı; hedef adres Google Maps'te açılıyor.";
        openRoute("",routeWindow);
      },
      {enableHighAccuracy:false,timeout:8000,maximumAge:300000}
    );
  });
});
