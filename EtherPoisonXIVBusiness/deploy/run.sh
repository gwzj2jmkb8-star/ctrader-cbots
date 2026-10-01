#!/bin/sh
set -eu

: "${CTID:?CTID is required}"
: "${ACCOUNT:?ACCOUNT is required}"
: "${SYMBOL:?SYMBOL is required}"
: "${PERIOD:?PERIOD is required}"
: "${CTRADER_PASSWORD:?CTRADER_PASSWORD secret is required}"

umask 077
password_file="/tmp/ether-ctrader-password"
printf '%s' "$CTRADER_PASSWORD" > "$password_file"
unset CTRADER_PASSWORD

exec ctrader-cli run /opt/ether/EtherPoisonXIVBusiness.algo \
  "--ctid=$CTID" "--pwd-file=$password_file" "--account=$ACCOUNT" \
  "--symbol=$SYMBOL" "--period=$PERIOD" --exit-on-stop
