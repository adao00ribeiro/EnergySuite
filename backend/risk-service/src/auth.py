import os
import jwt
from fastapi import Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer
import httpx

KEYCLOAK_URL = os.getenv("KEYCLOAK_URL", "http://keycloak:8080/realms/EnergySuite")
oauth2_scheme = OAuth2PasswordBearer(tokenUrl=f"{KEYCLOAK_URL}/protocol/openid-connect/token", auto_error=False)

async def verify_jwt(token: str = Depends(oauth2_scheme)):
    if not token:
        # Development fallback / optional token mode
        return {"sub": "anonymous", "roles": ["Executive"]}
    
    try:
        # Unverified header to get kid
        unverified_header = jwt.get_unverified_header(token)
        jwks_url = f"{KEYCLOAK_URL}/protocol/openid-connect/certs"
        
        async with httpx.AsyncClient() as client:
            response = await client.get(jwks_url)
            jwks = response.json()
            
        public_key = None
        for key in jwks.get("keys", []):
            if key["kid"] == unverified_header["kid"]:
                public_key = jwt.algorithms.RSAAlgorithm.from_jwk(key)
                break
                
        if not public_key:
            raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Invalid token key ID")
            
        payload = jwt.decode(token, public_key, algorithms=["RS256"], options={"verify_aud": False})
        return payload
    except Exception as e:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail=f"Token validation failed: {str(e)}")
