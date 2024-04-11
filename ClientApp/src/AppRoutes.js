import { Counter } from "./components/Counter";
import { WeatherForecast } from "./components/WeatherForecast";
import { Home } from "./components/Home";

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '/counter',
    element: <Counter />
  },
  {
    path: '/weather-forecast',
    element: <WeatherForecast />
  }
];

export default AppRoutes;
