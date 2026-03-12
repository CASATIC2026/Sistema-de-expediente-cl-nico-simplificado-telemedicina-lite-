<script setup>
import { ref } from 'vue';
</script>

<template>
<div class="contenedorPrincipal">
    <button>&times; cerrar</button>

    <div class="tarjeta">
        <h3>Ver mis citas</h3>

        <div class="items">
            <div>Pendientes</div>
            <div>Canceladas</div>
            <div>Finalizadas</div>


        </div>
        <div>
            <p>fecha: </p>
        </div>
        <div>
            <p>hora: </p>
        </div>
        <div>
            Dr/a:
        </div>
        <div>
           <p>Enlace : <a href="#">www.ejemplo.link.consulta.com</a></p>
        </div>

    </div>
</div>
</template>

<style>
</style>