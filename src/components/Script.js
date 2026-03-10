const selectRol = document.getElementById("Rol");
const divDoc = document.getElementById("DivDoc");
const divUsu = document.getElementById("DivUsu");
const divAdmin = document.getElementById("DivAdmin");

divDoc.style.display = "none";
divUsu.style.display = "none";
divAdmin.style.display = "none";

selectRol.addEventListener("change", function(){
    divDoc.style.display = "none";
    divUsu.style.display = "none";
    divAdmin.style.display = "none";

    if(this.value === "Doctor"){
        divDoc.style.display = "block";
    }
    
    if(this.value === "Usuario"){
        divUsu.style.display = "block";
    }
    
    if(this.value === "Admin"){
        divAdmin.style.display = "block";
    }
})