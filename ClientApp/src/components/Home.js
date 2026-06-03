import React from "react";
import { Link } from "react-router-dom";

export default function Home() {
  return (
    <div className="container py-5">
      <div className="p-5 mb-4 bg-light rounded-3">
        <h1 className="display-5 fw-bold">🌍 Welcome to Travel Bot</h1>
        <p className="col-md-8 fs-4">
          Explore the most populous countries in the Southern Hemisphere —
          complete with sunrise times, languages, and distance from Cape Town.
        </p>
        <Link to="/travelbot" className="btn btn-primary btn-lg">Explore Now →</Link>
      </div>
      <div className="row">
        <div className="col-md-4 mb-3">
          <div className="h-100 p-4 border rounded-3 shadow-sm">
            <h2>📊 Top 5 Countries</h2>
            <p>Ranked by population in the Southern Hemisphere.</p>
          </div>
        </div>
        <div className="col-md-4 mb-3">
          <div className="h-100 p-4 border rounded-3 shadow-sm">
            <h2>🌅 Sunrise & Sunset</h2>
            <p>Live sunrise and sunset times for each capital city (UTC).</p>
          </div>
        </div>
        <div className="col-md-4 mb-3">
          <div className="h-100 p-4 border rounded-3 shadow-sm">
            <h2>🎲 Surprise Me</h2>
            <p>Let Travel Bot pick a random destination for you.</p>
          </div>
        </div>
      </div>
    </div>
  );
}
