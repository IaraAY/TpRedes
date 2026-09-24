function Verificar()
{
    let usuario = document.getElementById("Usu").value.toLowerCase();
    let nombre = document.getElementById("Nom").value;
    let apellido = document.getElementById("Ap").value;
    let contraseña = document.getElementById("Cont").value;

    // Hacer que el regex no permita números ni caracteres especiales en el nombre y apellido pero permita espacios y letras acentuadas
    const Regex = /^[a-zA-Záéíóúñ\s]+$/;
    Rta.innerText = "";
    let verificacion = true;
    if(!Regex.test(nombre) || !Regex.test(apellido))
    {
        Rta.innerText += "\r\nEl nombre y el apellido no pueden contener números ni caracteres especiales.";
        verificacion = false;
    } 
    if(contraseña.length < 8)
    {
        Rta.innerText += "Contraseña de mínimo 8 caracteres."
        verificacion = false;
    }
    if(usuario.length < 4)
    {
        Rta.innerText += "Longitud mínima del nombre de usuario es de 4 caracteres."
        verificacion = false;
    }
    return verificacion;
}
//Hacer una funcion de java script que cuando se seleccione el país llame a la función ObtenerProvincias del HomeController y obtenga las provincias de ese país y las muestre en el dropdown de provincias.
    function obtenerProvincias(pais) {

        const selectProvincias = document.getElementById("provincias");

        if (pais === "") {
            selectProvincias.innerHTML =
                '<option value="">Seleccione un país primero</option>';

            selectProvincias.disabled = true;
            return;
        }

        fetch('/Home/ObtenerProvincias?pais=' + encodeURIComponent(pais))
            .then(response => response.json())
            .then(provincias => {

                selectProvincias.innerHTML =
                    '<option value="">Seleccione una provincia</option>';

                provincias.forEach(provincia => {

                    const option = document.createElement("option");

                    option.value = provincia;
                    option.textContent = provincia;

                    selectProvincias.appendChild(option);
                });

                selectProvincias.disabled = false;
            })
            .catch(error => {
                console.error("Error:", error);
            });
    }