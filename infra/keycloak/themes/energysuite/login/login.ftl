<#import "template.ftl" as layout>
<@layout.registrationLayout displayMessage=!messagesPerField.existsError('username','password') displayInfo=realm.password && realm.registrationAllowed && !registrationDisabled??; section>
    <#if section = "header">
    <#elseif section = "form">
    <div class="login-wrapper">
        <div class="ambient-glow"></div>
        <div class="mat-card login-card mat-elevation-z8">
            <div class="brand">
                <div class="logo-icon">
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="energy-bolt">
                        <path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z"/>
                    </svg>
                </div>
                <h2>EnergySuite</h2>
            </div>
            
            <div class="login-box">
                <h1 class="mat-headline-small">Acesse sua conta</h1>
                <p class="mat-body-medium">Insira suas credenciais para continuar.</p>

                <#if message?? && (message.type != 'warning' || !isAppInitiatedAction??)>
                    <div class="mat-alert mat-alert-${message.type}">
                        <div class="mat-alert-icon">
                            <#if message.type = 'error'>
                                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                    <circle cx="12" cy="12" r="10"></circle>
                                    <line x1="12" y1="8" x2="12" y2="12"></line>
                                    <line x1="12" y1="16" x2="12.01" y2="16"></line>
                                </svg>
                            <#else>
                                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                    <circle cx="12" cy="12" r="10"></circle>
                                    <line x1="12" y1="16" x2="12" y2="12"></line>
                                    <line x1="12" y1="8" x2="12.01" y2="8"></line>
                                </svg>
                            </#if>
                        </div>
                        <span class="mat-alert-text">${kcSanitize(message.summary)?no_esc}</span>
                    </div>
                </#if>

                <#if realm.password>
                    <form id="kc-form-login" class="mat-form-container" action="${url.loginAction}" method="post">
                        
                        <!-- Angular Material 3 Form Field: Username / Email -->
                        <div class="mat-form-field <#if messagesPerField.existsError('username','password')>mat-form-field-invalid</#if>">
                            <div class="mat-form-field-flex">
                                <div class="mat-form-field-prefix">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                        <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path>
                                        <polyline points="22,6 12,13 2,6"></polyline>
                                    </svg>
                                </div>
                                <div class="mat-form-field-infix">
                                    <input tabindex="1" id="username" class="mat-input" name="username" value="${(login.username!'')}" type="text" autofocus autocomplete="username" placeholder=" " required />
                                    <label for="username" class="mat-floating-label">Email ou Usuário</label>
                                </div>
                            </div>
                            <div class="mat-form-field-subscript">
                                <#if messagesPerField.existsError('username')>
                                    <span class="mat-error">${kcSanitize(messagesPerField.get('username'))?no_esc}</span>
                                </#if>
                            </div>
                        </div>

                        <!-- Angular Material 3 Form Field: Password -->
                        <div class="mat-form-field <#if messagesPerField.existsError('username','password')>mat-form-field-invalid</#if>">
                            <div class="mat-form-field-flex">
                                <div class="mat-form-field-prefix">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                        <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
                                        <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
                                    </svg>
                                </div>
                                <div class="mat-form-field-infix">
                                    <input tabindex="2" id="password" class="mat-input" name="password" type="password" autocomplete="current-password" placeholder=" " required />
                                    <label for="password" class="mat-floating-label">Senha</label>
                                </div>
                                <div class="mat-form-field-suffix">
                                    <button type="button" class="mat-icon-button toggle-password" onclick="togglePasswordVisibility()" title="Mostrar ou ocultar senha">
                                        <svg id="eye-icon" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                            <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                                            <circle cx="12" cy="12" r="3"></circle>
                                        </svg>
                                    </button>
                                </div>
                            </div>
                            <div class="mat-form-field-subscript">
                                <#if messagesPerField.existsError('password')>
                                    <span class="mat-error">${kcSanitize(messagesPerField.get('password'))?no_esc}</span>
                                </#if>
                            </div>
                        </div>

                        <!-- Actions: Forgot Password & Submit -->
                        <div class="form-actions">
                            <#if realm.resetPasswordAllowed>
                                <a tabindex="5" href="${url.loginResetCredentialsUrl}" class="mat-text-button forgot-pwd">Esqueceu a senha?</a>
                            </#if>
                            
                            <button tabindex="4" class="mat-button mat-raised-button mat-primary btn-submit" name="login" id="kc-login" type="submit">
                                <span>Entrar</span>
                                <svg class="btn-arrow" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                                    <line x1="5" y1="12" x2="19" y2="12"></line>
                                    <polyline points="12 5 19 12 12 19"></polyline>
                                </svg>
                            </button>
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
