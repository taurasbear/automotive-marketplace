import { Toaster } from "./components/ui/sonner";
import CarListings from "./containers/CarListings";
import Register from "./containers/Register";

function App() {
  return (
    <div>
      {/* <CarListings /> */}
      <Register/>
      <Toaster />
    </div>
  );
}

export default App;
