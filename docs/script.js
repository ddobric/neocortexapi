// var btnGs = document.getElementById("get-started").addEventListener("click", function() {
//     var element = document.getElementById("gs");
//     element.scrollIntoView({behavior: 'smooth'});
// });
// var btnSp = document.getElementById("spatial-pooler").addEventListener("click", function() {
//     var element = document.getElementById("sp");
//     element.scrollIntoView({behavior: 'smooth'});
// });
// var btnGs = document.getElementById("temporal-memory").addEventListener("click", function() {
//     var element = document.getElementById("tm");
//     element.scrollIntoView({behavior: 'smooth'});
// });

scrollBtn("get-started", "gs");
scrollBtn("spatial-pooler", "sp");
scrollBtn("temporal-memory", "tm");

// var btnGS = document.getElementById("spatial-pooler");
// btnGS.click()

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
        var link = holder.getElementsByTagName("a");
        if (link != null){
            for (let index = 0; index < link.length; index++) {
                const element = link[index];
                holder.removeChild(element);
            }
        }
    }
  } else {
      console.log("hello")
    for (var i = 0; i < imageHolders.length; i++) {
      var holder = imageHolders[i];
      var image = holder.getElementsByClassName("image")[0];
      if (image != null) {
        var link = document.createElement("a");
        link.href = image.src;
        link.target = "_blank";
        link.appendChild(document.createTextNode("Click to view original size."));
        holder.appendChild(link);
      }
    }
  }
}

var x = window.matchMedia("(min-width: 1000px)");
addImageLink(x); // Call listener function at run time
x.addListener(addImageLink);
