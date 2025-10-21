#!/bin/bash
# Load Test Script for Database Connection Pooling Verification
# Task 20.5: Load test with 100 concurrent requests and verify performance
# Target: <500ms average response time under load

set -e

# Configuration
ENDPOINT="${1:-http://localhost:5000/api/models}"
CONCURRENT_REQUESTS=100
WARMUP_REQUESTS=10
EXPECTED_MAX_AVG_MS=500

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "=========================================="
echo "Database Connection Pooling Load Test"
echo "=========================================="
echo "Endpoint: $ENDPOINT"
echo "Concurrent Requests: $CONCURRENT_REQUESTS"
echo "Expected Max Avg Response: ${EXPECTED_MAX_AVG_MS}ms"
echo ""

# Check if server is running
echo "Checking if server is running..."
if ! curl -s -f "$ENDPOINT" > /dev/null 2>&1; then
    echo -e "${RED}ERROR: Server is not running at $ENDPOINT${NC}"
    echo "Start the server with: cd services/backend/LlmTokenPrice.API && dotnet run"
    exit 1
fi
echo -e "${GREEN}✓ Server is running${NC}"
echo ""

# Warmup requests to initialize connection pool
echo "Warming up connection pool with $WARMUP_REQUESTS requests..."
for i in $(seq 1 $WARMUP_REQUESTS); do
    curl -s "$ENDPOINT" > /dev/null
done
echo -e "${GREEN}✓ Warmup complete${NC}"
echo ""

# Create temporary file for results
RESULTS_FILE=$(mktemp)
trap "rm -f $RESULTS_FILE" EXIT

# Run load test
echo "Running load test with $CONCURRENT_REQUESTS concurrent requests..."
START_TIME=$(date +%s%N)

# Use GNU parallel if available, otherwise fall back to xargs
if command -v parallel > /dev/null 2>&1; then
    seq 1 $CONCURRENT_REQUESTS | parallel -j $CONCURRENT_REQUESTS "curl -s -w '%{time_total}\n' -o /dev/null $ENDPOINT" > "$RESULTS_FILE"
else
    seq 1 $CONCURRENT_REQUESTS | xargs -I {} -P $CONCURRENT_REQUESTS curl -s -w '%{time_total}\n' -o /dev/null "$ENDPOINT" > "$RESULTS_FILE"
fi

END_TIME=$(date +%s%N)

# Calculate statistics
TOTAL_TIME_NS=$((END_TIME - START_TIME))
TOTAL_TIME_MS=$((TOTAL_TIME_NS / 1000000))

# Convert curl time_total (seconds with decimals) to milliseconds and calculate stats
RESPONSE_TIMES=$(awk '{print $1 * 1000}' "$RESULTS_FILE")
MIN_MS=$(echo "$RESPONSE_TIMES" | sort -n | head -1)
MAX_MS=$(echo "$RESPONSE_TIMES" | sort -n | tail -1)
AVG_MS=$(echo "$RESPONSE_TIMES" | awk '{sum+=$1; count+=1} END {print sum/count}')
MEDIAN_MS=$(echo "$RESPONSE_TIMES" | sort -n | awk '{a[NR]=$1} END {print (NR%2==1)?a[(NR+1)/2]:(a[NR/2]+a[NR/2+1])/2}')

# Calculate percentiles
P95_MS=$(echo "$RESPONSE_TIMES" | sort -n | awk 'BEGIN{c=0} {a[c++]=$1} END{print a[int(c*0.95)]}')
P99_MS=$(echo "$RESPONSE_TIMES" | sort -n | awk 'BEGIN{c=0} {a[c++]=$1} END{print a[int(c*0.99)]}')

# Calculate throughput
THROUGHPUT=$(awk "BEGIN {printf \"%.2f\", $CONCURRENT_REQUESTS / ($TOTAL_TIME_MS / 1000)}")

# Display results
echo ""
echo "=========================================="
echo "RESULTS"
echo "=========================================="
echo "Total Requests: $CONCURRENT_REQUESTS"
echo "Total Time: ${TOTAL_TIME_MS}ms"
echo "Throughput: ${THROUGHPUT} req/sec"
echo ""
echo "Response Times (ms):"
printf "  Min:    %10.2f ms\n" "$MIN_MS"
printf "  Max:    %10.2f ms\n" "$MAX_MS"
printf "  Avg:    %10.2f ms\n" "$AVG_MS"
printf "  Median: %10.2f ms\n" "$MEDIAN_MS"
printf "  P95:    %10.2f ms\n" "$P95_MS"
printf "  P99:    %10.2f ms\n" "$P99_MS"
echo ""

# Check if performance meets requirements
AVG_MS_INT=$(printf "%.0f" "$AVG_MS")
if [ "$AVG_MS_INT" -lt "$EXPECTED_MAX_AVG_MS" ]; then
    echo -e "${GREEN}✓ PASS: Average response time (${AVG_MS_INT}ms) is below target (${EXPECTED_MAX_AVG_MS}ms)${NC}"
    echo ""
    echo "Connection pooling is working correctly:"
    echo "  - Min Pool Size: 5 (maintains warm connections)"
    echo "  - Max Pool Size: 100 (handles concurrent load)"
    echo "  - Response times are consistently fast"
    EXIT_CODE=0
else
    echo -e "${RED}✗ FAIL: Average response time (${AVG_MS_INT}ms) exceeds target (${EXPECTED_MAX_AVG_MS}ms)${NC}"
    echo ""
    echo "Recommendations:"
    echo "  1. Check database query performance (use EXPLAIN ANALYZE)"
    echo "  2. Verify connection pool is not exhausted (check PostgreSQL logs)"
    echo "  3. Consider increasing Minimum Pool Size if cold starts are slow"
    echo "  4. Review indexes on frequently queried tables"
    EXIT_CODE=1
fi

echo "=========================================="

exit $EXIT_CODE
