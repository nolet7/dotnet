import http from "k6/http";
import { check } from "k6";

export let options = {
  vus: 10,
  duration: "1m"
};

export default function () {
  let res = http.post("http://apigateway/orders");
  check(res, {
    "status is 200": (r) => r.status === 200
  });
}
