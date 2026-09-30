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

