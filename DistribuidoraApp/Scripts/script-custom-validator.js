$(document).ready(function () {
    $('.form').validate({
        errorClass: 'help-block animation-slideDown', // You can change the animation class for a different entrance animation - check animations page  
        errorElement: 'div',
        errorPlacement: function (error, e) {
            e.parents('.form-group > div').append(error);
        },
        highlight: function (e) {

            $(e).closest('.form-group').removeClass('has-success has-error').addClass('has-error');
            $(e).closest('.help-block').remove();
        },
        success: function (e) {
            e.closest('.form-group').removeClass('has-success has-error');
            e.closest('.help-block').remove();
        },
        rules: {
            'RazonSocial': {
                required: true
            },

            'CUIT': {
                required: true
            },

            'CondicionIVAId': {
                required: true
            }
        },
        messages: {
            'RazonSocial': 'Ingresar una razón social',

            'CUIT': {
                required: 'Ingresar un CUIT válido'
            },

            'CondicionIVAId': {
                required: 'Seleccionar la condición frente al IVA'
            }
        }
    });
});   