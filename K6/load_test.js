// load_test.js
import http from 'k6/http';
import grpc from 'k6/net/grpc';
import { check, sleep } from 'k6';

// Prepare gRPC client
const client = new grpc.Client();
client.load(['.'], 'sum.proto');

export let options = {
  scenarios: {
    http_load: {
      executor: 'constant-vus',
      vus: 50,
      duration: '60s',
    },
    grpc_load: {
      executor: 'constant-vus',
      vus: 30,
      duration: '45s',
    },
  },
};

export default function () {
  // HTTP GET
  let res = http.get('http://localhost:5270/api/Agent');
  check(res, {
    'HTTP status is 200': (r) => r.status === 200,
  });

  // gRPC AddNumbers
  client.connect('localhost:7168', { plaintext: true });
  let grpcRes = client.invoke('SumService/AddNumbers', { number1: 2, number2: 3 });
  check(grpcRes, {
    'gRPC status OK': (r) => r && r.status === grpc.StatusOK,
    'result is bool': (r) => typeof r.message.result === 'boolean',
  });
  client.close();

  sleep(1);
}
