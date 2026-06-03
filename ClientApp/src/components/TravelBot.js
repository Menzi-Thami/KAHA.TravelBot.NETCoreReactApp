import React, { useState, useEffect, useCallback, memo } from "react";

// ── API routes ────────────────────────────────────────────────────────────────
const API = {
  top5:     "/api/countries/top5",
  summary:  (name) => `/api/countries/summary?countryName=${encodeURIComponent(name)}`,
  surprise: "/api/countries/surprise",
};

// ── Shared fetch helper ───────────────────────────────────────────────────────
async function apiFetch(url) {
  const res = await fetch(url);
  if (!res.ok) throw new Error(`HTTP ${res.status}: ${res.statusText}`);
  return res.json();
}

// ── Sub-components (memoised to avoid unnecessary re-renders) ─────────────────

const LoadingSpinner = memo(function LoadingSpinner({ text = "Loading…" }) {
  return (
    <div className="d-flex align-items-center gap-2 my-3" role="status" aria-live="polite">
      <div className="spinner-border spinner-border-sm text-primary" aria-hidden="true" />
      <span className="text-muted">{text}</span>
    </div>
  );
});

const ErrorAlert = memo(function ErrorAlert({ message, onDismiss }) {
  if (!message) return null;
  return (
    <div className="alert alert-danger d-flex justify-content-between align-items-center"
         role="alert" aria-live="assertive">
      <span>⚠️ {message}</span>
      {onDismiss && (
        <button className="btn-close" aria-label="Dismiss error" onClick={onDismiss} />
      )}
    </div>
  );
});

const TopFiveTable = memo(function TopFiveTable({
  countries, onCountryClick, selectedName, loadingName
}) {
  if (!countries.length) return null;

  return (
    <div className="table-responsive">
      <table className="table table-hover align-middle" aria-label="Top 5 Southern Hemisphere countries by population">
        <thead className="table-dark">
          <tr>
            <th scope="col">#</th>
            <th scope="col">Country</th>
            <th scope="col">Capital</th>
            <th scope="col">Population</th>
            <th scope="col">Lat / Lon</th>
          </tr>
        </thead>
        <tbody>
          {countries.map((c, i) => (
            <tr
              key={c.name}
              onClick={() => onCountryClick(c.name)}
              onKeyDown={(e) => e.key === "Enter" && onCountryClick(c.name)}
              tabIndex={0}
              role="button"
              aria-pressed={selectedName === c.name}
              aria-label={`View details for ${c.name}`}
              style={{ cursor: "pointer" }}
              className={selectedName === c.name ? "table-primary" : ""}
            >
              <td>{i + 1}</td>
              <td>
                <strong>{c.name}</strong>
                {loadingName === c.name && (
                  <span
                    className="spinner-border spinner-border-sm ms-2 text-secondary"
                    role="status"
                    aria-label="Loading country details"
                  />
                )}
              </td>
              <td>{c.capital}</td>
              <td>{c.population.toLocaleString()}</td>
              <td className="text-muted" style={{ fontSize: "0.85em" }}>
                {c.latitude.toFixed(2)}°, {c.longitude.toFixed(2)}°
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <p className="text-muted small">Click or press Enter on a row to view full country details.</p>
    </div>
  );
});

const CountrySummaryCard = memo(function CountrySummaryCard({ summary, onClose }) {
  if (!summary) return null;

  const rows = [
    { label: "🏙️ Capital",            value: summary.capital },
    { label: "👥 Population",          value: summary.population.toLocaleString() },
    { label: "🌅 Sunrise (UTC)",       value: summary.sunrise },
    { label: "🌇 Sunset (UTC)",        value: summary.sunset },
    { label: "🗣️ Official Languages",  value: summary.officialLanguages },
    { label: "📚 Total Languages",     value: summary.totalLanguages },
    { label: "🚗 Drive Side",          value: summary.driveSide },
    { label: "📍 Distance from KAHA",  value: `${summary.distanceFromKahaKm.toLocaleString()} km` },
  ];

  return (
    <div className="card border-primary mt-4" role="region" aria-label={`Details for ${summary.name}`}>
      <div className="card-header bg-primary text-white d-flex justify-content-between align-items-center">
        <h5 className="mb-0">🌍 {summary.name}</h5>
        <button className="btn btn-sm btn-light" onClick={onClose} aria-label="Close country details">
          ✕ Close
        </button>
      </div>
      <div className="card-body p-0">
        <table className="table table-sm mb-0" aria-label={`${summary.name} country details`}>
          <tbody>
            {rows.map((r) => (
              <tr key={r.label}>
                <th scope="row" style={{ width: "42%" }} className="ps-3">{r.label}</th>
                <td>{r.value}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
});

// ── Main TravelBot page ───────────────────────────────────────────────────────

export default function TravelBot() {
  const [topFive,          setTopFive]          = useState([]);
  const [summary,          setSummary]          = useState(null);
  const [selectedName,     setSelectedName]     = useState("");
  const [loadingName,      setLoadingName]      = useState("");
  const [tableLoading,     setTableLoading]     = useState(true);
  const [surpriseLoading,  setSurpriseLoading]  = useState(false);
  const [error,            setError]            = useState("");

  // Fetch top 5 on mount
  useEffect(() => {
    apiFetch(API.top5)
      .then(setTopFive)
      .catch(() => setError("Failed to load top 5 countries. Please refresh the page."))
      .finally(() => setTableLoading(false));
  }, []);

  // Toggle country detail on row click
  const handleCountryClick = useCallback(async (name) => {
    if (name === selectedName) {
      setSummary(null);
      setSelectedName("");
      return;
    }
    setLoadingName(name);
    setError("");
    try {
      const data = await apiFetch(API.summary(name));
      setSummary(data);
      setSelectedName(name);
    } catch {
      setError(`Could not load details for "${name}". Please try again.`);
    } finally {
      setLoadingName("");
    }
  }, [selectedName]);

  // Surprise Me
  const handleSurprise = useCallback(async () => {
    setSurpriseLoading(true);
    setError("");
    setSummary(null);
    setSelectedName("");
    try {
      const data = await apiFetch(API.surprise);
      setSummary(data);
      setSelectedName(data.name);
    } catch {
      setError("Could not fetch a random country. Please try again.");
    } finally {
      setSurpriseLoading(false);
    }
  }, []);

  return (
    <div className="container py-4">

      {/* Header row */}
      <div className="d-flex align-items-start justify-content-between mb-2 flex-wrap gap-2">
        <div>
          <h2 className="mb-0">🌍 Travel Bot</h2>
          <p className="text-muted mb-0">
            Top 5 most populous countries in the Southern Hemisphere.
            Click any row for full details.
          </p>
        </div>
        <button
          className="btn btn-warning fw-semibold"
          onClick={handleSurprise}
          disabled={surpriseLoading}
          aria-label="Show a random Southern Hemisphere country"
        >
          {surpriseLoading
            ? <><span className="spinner-border spinner-border-sm me-2" aria-hidden="true" />Loading…</>
            : "🎲 Surprise Me!"}
        </button>
      </div>

      <hr />

      <ErrorAlert message={error} onDismiss={() => setError("")} />

      {tableLoading
        ? <LoadingSpinner text="Fetching top 5 countries…" />
        : <TopFiveTable
            countries={topFive}
            onCountryClick={handleCountryClick}
            selectedName={selectedName}
            loadingName={loadingName}
          />
      }

      {summary && (
        <CountrySummaryCard
          summary={summary}
          onClose={() => { setSummary(null); setSelectedName(""); }}
        />
      )}

    </div>
  );
}
