import Home from "./components/Home";
import TravelBot from "./components/TravelBot";
import TravelBotTable from "./components/TravelBotTable";

const AppRoutes = [
  {
    path: "/",
    element: <Home />,
    index: true
  },
  {
    path: "/TravelBot",
    element: <TravelBot />
  },
  {
    path: "/TravelBotTable",
    element: <TravelBotTable />
  }
];

export default AppRoutes;
