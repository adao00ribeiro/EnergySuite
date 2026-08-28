<#import "template.ftl" as layout>
<@layout.registrationLayout displayMessage=!messagesPerField.existsError('username','password') displayInfo=realm.password && realm.registrationAllowed && !registrationDisabled??; section>
    <#if section = "header">
    <#elseif section = "form">
    <div class="login-wrapper">
        <div class="login-card">
            <div class="brand">
                <div class="logo-icon"></div>
                <h2>EnergySuite</h2>
            </div>
            
            <div class="login-box">
                <h1>Acesse sua conta</h1>
                <p>Insira suas credenciais para continuar.</p>

                <#if realm.password>
                    <form id="kc-form-login" onsubmit="login.disabled = true; return true;" action="${url.loginAction}" method="post">
                        <div class="form-group">
                            <label for="username" class="sr-only">${msg("usernameOrEmail")}</label>
                            <input tabindex="1" id="username" class="form-control" name="username" value="${(login.username!'')}"  type="text" autofocus autocomplete="off" placeholder="Email" />
                        </div>

                        <div class="form-group password-group">
                            <label for="password" class="sr-only">${msg("password")}</label>
                            <input tabindex="2" id="password" class="form-control" name="password" type="password" autocomplete="off" placeholder="Senha" />
                            <button type="button" class="toggle-password" onclick="togglePasswordVisibility()" title="Mostrar senha">
                                <svg id="eye-icon" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                    <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                                    <circle cx="12" cy="12" r="3"></circle>
                                </svg>
                            </button>
                        </div>

                        <div class="form-actions">
                            <#if realm.resetPasswordAllowed>
                                <a tabindex="5" href="${url.loginResetCredentialsUrl}" class="forgot-pwd">Esqueceu a senha?</a>
                            </#if>
                            <button tabindex="4" class="btn-submit" name="login" id="kc-login" type="submit">Entrar</button>
                        </div>
                    </form>
                </#if>
            </div>
        </div>
    </div>

    <script>
        function togglePasswordVisibility() {
            var pwdInput = document.getElementById("password");
            var eyeIcon = document.getElementById("eye-icon");
            if (pwdInput.type === "password") {
                pwdInput.type = "text";
                eyeIcon.innerHTML = '<path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path><line x1="1" y1="1" x2="23" y2="23"></line>';
            } else {
                pwdInput.type = "password";
                eyeIcon.innerHTML = '<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle>';
            }
        }
    </script>
    </#if>
</@layout.registrationLayout>
