import { Toaster } from "./components/ui/sonner";
import { RouterProvider } from "@tanstack/react-router";
import { router } from "./lib/router";

function App() {
  return (
    <div>
      <RouterProvider router={router} />
      <Toaster />
    </div>
  );
}

export default App;
