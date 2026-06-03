import Home      from "./components/Home";
import TravelBot from "./components/TravelBot";

const AppRoutes = [
  { path: "/",          element: <Home />,      index: true },
  { path: "/travelbot", element: <TravelBot /> }
];

export default AppRoutes;
