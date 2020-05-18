from flask import Flask, request, jsonify
from flask_login import login_user
from flask_jwt_extended import JWTManager, jwt_required, create_access_token, create_refresh_token, get_jwt_identity, jwt_refresh_token_required
from cryptography import x509
from cryptography.hazmat.backends import default_backend
import requests
from werkzeug.security import generate_password_hash, check_password_hash
import jwt as jwtStandalone
import base64
import datetime

app = Flask(__name__)
app.debug = True
app.config['JWT_SECRET_KEY'] = ''
app.config['SAFETYNET_KEY'] = ''

mathpix_api = {
    'Content-Type': 'application/json',
    'app_id': '',
    'app_key': ''
}

jwt = JWTManager(app)


@app.route('/latex', methods=['POST'])
@jwt_required
def latexImageProcess():
    if 'Device-id' in request.headers:
        current_user = get_jwt_identity()
        if(request.headers['Device-id'] == current_user):
            if 'images' in request.get_json():
                results = []
                for image in request.get_json()['images']:
                    resp = requests.post("https://api.mathpix.com/v3/text", headers=mathpix_api, json={
                        'src': image, 'metadata': {'improve_mathpix': False}})
                    results.append(resp.json())
                return jsonify({"output": results})
            return "Missing field src", 400
        return "Unathorized", 401
    return "Missing Device-id header", 400


@app.route('/createtoken', methods=['POST'])
def getJWTTokens():
    jsonData = request.get_json()
    if jsonData and 'safetynet' in jsonData and 'device_id' in jsonData and 'timestamp' in jsonData:
        jwt_body = jsonData['safetynet']

        x5c = jwtStandalone.get_unverified_header(jwt_body)['x5c'][0]
        certificate = x509.load_der_x509_certificate(
            base64.b64decode(x5c), default_backend())

        if "CN=attest.android.com" not in certificate.subject.rfc4514_string():
            return "Attestation failed!", 403

        jwt_decoded = jwtStandalone.decode(
            jwt_body, key=certificate.public_key(), algorithm='HS256')

        if jwt_decoded and 'nonce' in jwt_decoded:
            if not check_password_hash(base64.b64decode(jwt_decoded['nonce']).decode('utf-8'), jsonData['timestamp']+jsonData['device_id']):
                return "Error with nonce!", 403

        if jwt_decoded and 'ctsProfileMatch' in jwt_decoded and 'basicIntegrity' in jwt_decoded:
            if jwt_decoded['ctsProfileMatch'] or jwt_decoded['basicIntegrity']:
                expires = datetime.timedelta(days=30)
                jwt_keys = {
                    'access_token': create_access_token(identity=jsonData['device_id']),
                    'refresh_token': create_refresh_token(identity=jsonData['device_id'], expires_delta=expires)
                }
                return jsonify(jwt_keys)
            else:
                return "Attestation failed!", 403
        else:
            return "Invalid attestation response", 403
    return "Missing body parts!", 403


@app.route('/getnonce', methods=['POST'])
def getSafetyNetNonce():
    req = request.get_json()
    if req and 'timestamp' in req and 'device_id' in req:
        return generate_password_hash(req['timestamp']+req['device_id'])
    return "Missing body parts! Body must include timestamp and device_id string"


@app.route('/', methods=['GET', 'POST'])
def hello_world():
    req = request.get_json()
    if req and 'safetynet' in req:
        jwtRaw = req['safetynet']
        res = jwtStandalone.decode(jwtRaw, verify=False, algorithm='HS256')
        return jsonify(res)
    return "Missing safetynet part!", 404


@app.route('/refresh', methods=['POST'])
@jwt_refresh_token_required
def refreshJWTtokens():
    if 'Device-id' in request.headers:
        current_user = get_jwt_identity()
        if(request.headers['Device-id'] == current_user):
            expires = datetime.timedelta(days=30)
            ret = {
                'access_token': create_access_token(identity=current_user, expires_delta=datetime.timedelta(seconds=20)),
                'refresh_token': create_refresh_token(identity=current_user, expires_delta=expires)
            }
            return jsonify(ret), 200
        return "Unathorized", 401
    return "Missing Device-id header", 400


if __name__ == "__main__":
    app.run()
