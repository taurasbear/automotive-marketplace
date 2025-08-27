import { Toaster } from "@/components/ui/sonner";
import { router } from "@/lib/router";
import { RouterProvider } from "@tanstack/react-router";

function App() {
  return (
    <div>
      <RouterProvider router={router} />
      <Toaster />
    </div>
  );
}

export default App;
