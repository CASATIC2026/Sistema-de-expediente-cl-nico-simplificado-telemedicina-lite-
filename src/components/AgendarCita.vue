<template>
     <div class="contenedorCitas">
         <button type="button" class="botonCerrar" onclick="">&times;</button>
        <p><strong>Complete los campos para agendar una cita</strong></p>
        <form action="link donde se enviará la informacion">
             <div class="campoCitas">
                <label for="doctorID">Seleccione un Especialista</label>
                <select name="doctorID" id="doctorID" required>
                    <option value="" disabled selected>--Seleccione un doctor--</option>
                    <option value="1">Doctor xyz</option>
                    <option value="2">Doctor aeiou</option>
                    <option value="3">Doctor xyz</option>
                    <option value="4">Doctor abcde</option>
                </select>
            </div>
             <div class="campoCitas">
                <label for="fecha">Seleccionar fecha</label>
                <input type="date" id="fecha" name="fecha" placeholder="Ingresar fecha" required>
            </div>
             <div>
                <label for="hora">Seleccionar hora disponible</label>
                <select name="hora" id="hora" required>
                    <option value="" disabled selected>---Primero elija un doctor---</option>
                    <option value="1">acá aparecerán las horas disponibles</option>
                    <option value="2">acá aparecerán las horas disponibles</option>
                    <option value="3">acá aparecerán las horas disponibles</option>
                </select>
            </div>
            <input type="hidden" name="estado" value="Pendiente">
            <button type="submit">Generar cita</button>
        </form>
    </div>
</template>