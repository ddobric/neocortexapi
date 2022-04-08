scrollBtn("get-started", "gs");
scrollBtn("spatial-pooler", "sp");
scrollBtn("temporal-memory", "tm");

function scrollBtn(btnId, targetId) {
  var btn = document.getElementById(btnId);
  btn.addEventListener("click", function () {
    var target = document.getElementById(targetId);
    target.scrollIntoView({ behavior: "smooth" });
  });
}

function addImageLink(x) {
  var imageHolders = document.getElementsByClassName("img-holder");
  if (x.matches) {
    for (var i = 0; i < imageHolders.length; i++) {
      var holder = imageHolders[i];
      var link = holder.getElementsByClassName("optional");
      if (link != null) {
        for (let index = 0; index < link.length; index++) {
          const element = link[index];
          holder.removeChild(element);
        }
      }
    }
  } else {
    for (var i = 0; i < imageHolders.length; i++) {
      var holder = imageHolders[i];
      var image = holder.getElementsByClassName("image")[0];
      if (image != null) {
        var optionalElement = document.createElement("div");
        optionalElement.classList.add("optional");
        optionalElement.classList.add("read-more");

        var link = document.createElement("a");
        link.href = image.src;
        link.target = "_blank";
        link.appendChild(
          document.createTextNode("Click to view original size.")
        );
        optionalElement.appendChild(link);
        holder.appendChild(optionalElement);
      }
    }
  }
}

function replaceBanner(x) {
  var element = document.getElementById("banner-img");
  if (x.matches) {
    element.src = "./images/Header-banner.svg";
  } else {
    element.src = "./images/Neocortex_smartphone.svg";
  }
}

var hamburger = document.getElementsByClassName("hamburger")[0];
if (hamburger != undefined) {
  hamburger.addEventListener("click", function () {
    hamburger.classList.toggle("open");
    var navElement = document.getElementsByClassName("nav-element-mobile")[0];
    navElement.classList.toggle("collapse");
  });
}

var x = window.matchMedia("(min-width: 1000px)");
addImageLink(x); // Call listener function at run time
replaceBanner(x);
x.addEventListener(addImageLink);
x.addEventListener(replaceBanner);
