FROM quay.io/keycloak/keycloak:22.0.1
# Copia o tema customizado
COPY themes/ /opt/keycloak/themes/
# Copia o realm para auto-importação (caso a base seja recriada)
COPY energysuite-realm.json /opt/keycloak/data/import/

