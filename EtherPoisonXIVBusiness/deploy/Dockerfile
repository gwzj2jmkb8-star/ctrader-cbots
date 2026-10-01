ARG CTRADER_BASE=ghcr.io/spotware/ctrader-console:5.9.11
FROM ${CTRADER_BASE}

# The .algo must be produced by a successful cTrader build before image creation.
COPY artifacts/EtherPoisonXIVBusiness.algo /opt/ether/EtherPoisonXIVBusiness.algo
COPY run.sh /opt/ether/run.sh
RUN chmod 500 /opt/ether/run.sh && chmod 444 /opt/ether/EtherPoisonXIVBusiness.algo
ENTRYPOINT ["/bin/sh", "/opt/ether/run.sh"]
